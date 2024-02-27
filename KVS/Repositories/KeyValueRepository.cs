using KVS.Data;
using KVS.Errors;
using OneOf;
using OneOf.Types;
using System.Diagnostics.CodeAnalysis;

namespace KVS.Repositories;

// FIXME: Validation for strings (no nulls)
public sealed class KeyValueRepository(IKeyValueCache _cache, IKeyValueDatabase _database) : IKeyValueRepository
{
    public OneOf<Success, AlreadyPresentError> AddKeyValue(string key, string value)
    {
        if (IsKeyInCache(key))
        {
            return new AlreadyPresentError();
        }

        // FIXME: This shouldn't ever return false, but we should probably handle that case
        _cache.TryAdd(key, value);
        _database.Add(key, value);

        return new Success();
    }

    public OneOf<Success<string>, NotFound> GetValueByKey(string key)
    {
        if (_cache.TryGetValue(key, out var value))
        {
            return new Success<string>(value);
        }
        // FIXME: We probably want a cache "dirty" flag so we don't always try to read from the database if a key doesn't exist
        if (TryReadKeyValueFromPersistance(key, out value))
        {
            return new Success<string>(value);
        }

        return new NotFound();
    }

    private bool TryReadKeyValueFromPersistance(string key, [MaybeNullWhen(false)] out string value)
    {
        if (!_database.TryGet(key, out value))
        {
            return false;
        }

        _cache.TryAdd(key, value);
        return true;
    }

    public OneOf<Success, NotFound> RemoveByKey(string key)
    {
        if (!IsKeyInCache(key))
        {
            return new NotFound();
        }

        _cache.Remove(key);
        _database.Delete(key);

        return new Success();
    }

    public OneOf<Success, NotFound> UpdateKeyValue(string key, string newValue)
    {
        if (!IsKeyInCache(key))
        {
            return new NotFound();
        }

        _cache[key] = newValue;
        _database.Update(key, newValue);

        return new Success();
    }

    private bool IsKeyInCache(string key) => _cache.ContainsKey(key);

    public IReadOnlyDictionary<string, string> KeyValueCache { get => _cache.KeyValues; }
}
