using KVS.Errors;
using KVS.Messages;
using MassTransit;
using OneOf;
using OneOf.Types;

namespace KVS.Repositories;

public enum CacheState
{
    Ok,
    Modified,
    Deleted
}

public sealed class KeyValueRepository(ILogger<KeyValueRepository> _logger, IKeyValueCache _cache, IKeyValueDatabase _database, IBus _bus) : IKeyValueRepository
{
    public async Task<OneOf<Success, AlreadyPresentError>> AddKeyValueAsync(string key, string value)
    {
        await UpdateValueIfDirtyCacheValue(key);

        if (IsKeyInCache(key))
        {
            return new AlreadyPresentError();
        }

        _cache.Add(key, value);
        await _database.AddAsync(key, value);

        // FIXME: What if our message publishing fails, but we have already modified the database?
        await PublishKeyModifiedMessageAsync(key);

        return new Success();
    }

    public async Task<OneOf<Success<string>, NotFound>> GetValueByKeyAsync(string key)
    {
        await UpdateValueIfDirtyCacheValue(key);

        if (_cache.TryGetValue(key, out var value))
        {
            return new Success<string>(value);
        }

        return new NotFound();
    }

    public async Task<OneOf<Success, NotFound>> RemoveByKeyAsync(string key)
    {
        await UpdateValueIfDirtyCacheValue(key);

        if (!IsKeyInCache(key))
        {
            return new NotFound();
        }

        _cache.Remove(key);
        await _database.DeleteAsync(key);

        await PublishKeyDeletionMessageAsync(key);

        return new Success();
    }

    public async Task<OneOf<Success, NotFound>> UpdateKeyValueAsync(string key, string newValue)
    {
        await UpdateValueIfDirtyCacheValue(key);

        if (!IsKeyInCache(key))
        {
            return new NotFound();
        }

        _cache[key] = newValue;
        await _database.UpdateAsync(key, newValue);

        await PublishKeyModifiedMessageAsync(key);

        return new Success();
    }

    private async Task PublishKeyModifiedMessageAsync(string key)
    {
        _logger.LogInformation("Trying to publish KeyModified message with the key '{ModifiedKey}'", key);
        await _bus.Publish(new KeyModified(key));
    }

    private async Task PublishKeyDeletionMessageAsync(string key)
    {
        _logger.LogInformation("Trying to publish KeyDeletion message with the key '{DeletedKey}'", key);
        await _bus.Publish(new KeyDeletion(key));
    }

    public void SetCacheFlagToModified(string modifiedKey)
    {
        if (!_keyToState.TryAdd(modifiedKey, CacheState.Modified))
        {
            _keyToState[modifiedKey] = CacheState.Modified;
        }
    }

    public void SetCacheFlagToDeleted(string deletedKey)
    {
        if (!_keyToState.TryAdd(deletedKey, CacheState.Deleted))
        {
            _keyToState[deletedKey] = CacheState.Deleted;
        }
    }

    private async Task UpdateValueIfDirtyCacheValue(string key)
    {
        if (!IsDirtyCacheForKey(key))
        {
            return;
        }

        if (_keyToState[key] == CacheState.Modified)
        {
            await HandleKeyModification(key);
        }
        else
        {
            HandleKeyDeletion(key);
        }

        _keyToState[key] = CacheState.Ok;
    }

    private async Task HandleKeyModification(string key)
    {
        _logger.LogInformation("Handling modified key for {Key}", key);

        var (exists, newValue) = await _database.TryGetAsync(key);
        if (!exists)
        {
            _logger.LogError("Handling key modification, but was unable to get the key {Key} from the database", key);
            return;
        }

        _cache.UpdateOrAdd(key, newValue!);
    }

    private void HandleKeyDeletion(string key)
    {
        _cache.Remove(key);
    }

    private bool IsDirtyCacheForKey(string key)
    {
        if (_keyToState.TryGetValue(key, out var cacheValue))
        {
            return cacheValue != CacheState.Ok;
        }

        return false;
    }

    private bool IsKeyInCache(string key) => _cache.ContainsKey(key);

    public IReadOnlyDictionary<string, string> KeyValueCache { get => _cache.KeyValues; }
    private readonly Dictionary<string, CacheState> _keyToState = [];
}
