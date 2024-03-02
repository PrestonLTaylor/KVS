using KVS.Errors;
using KVS.Messages;
using MassTransit;
using OneOf;
using OneOf.Types;

namespace KVS.Repositories;

public sealed class KeyValueRepository(ILogger<KeyValueRepository> _logger, IKeyValueCache _cache, IKeyValueDatabase _database, IBus _bus) : IKeyValueRepository
{
    public async Task<OneOf<Success, AlreadyPresentError>> AddKeyValueAsync(string key, string value)
    {
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
        if (_cache.TryGetValue(key, out var value))
        {
            return new Success<string>(value);
        }
        // FIXME: We probably want a cache "dirty" flag so we don't always try to read from the database if a key doesn't exist
        (var exists, value) = await TryReadKeyValueFromPersistanceAsync(key);
        if (exists)
        {
            return new Success<string>(value!);
        }

        return new NotFound();
    }

    private async Task<(bool, string?)> TryReadKeyValueFromPersistanceAsync(string key)
    {
        var (exists, value) = await _database.TryGetAsync(key);
        if (exists)
        {
            _cache.Add(key, value!);
        }

        return (exists, value);
    }

    public async Task<OneOf<Success, NotFound>> RemoveByKeyAsync(string key)
    {
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

    private bool IsKeyInCache(string key) => _cache.ContainsKey(key);

    public IReadOnlyDictionary<string, string> KeyValueCache { get => _cache.KeyValues; }
}
