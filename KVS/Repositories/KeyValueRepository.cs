using KVS.Data;
using KVS.Errors;
using OneOf;
using OneOf.Types;
using System.Diagnostics.CodeAnalysis;

namespace KVS.Repositories;

// FIXME: Validation for strings (no nulls)
public sealed class KeyValueRepository(IKeyValueCache _cache, DatabaseContext _db) : IKeyValueRepository
{
    public OneOf<Success, AlreadyPresentError> AddKeyValue(string key, string value)
    {
        if (IsKeyInCache(key))
        {
            return new AlreadyPresentError();
        }

        // FIXME: This shouldn't ever return false, but we should probably handle that case
        _cache.TryAdd(key, value);
        AddKeyValueToPersistance(key, value);

        return new Success();
    }

    private void AddKeyValueToPersistance(string key, string value)
    {
        // FIXME: Check if the key value was actually added and return an error
        // FIXME: Convert functions to async functions
        _db.KeyValues.Add(new() { Key = key, Value = value });
        _db.SaveChanges();
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
        var kv = _db.KeyValues.FirstOrDefault(kv => kv.Key == key);
        if (kv is null)
        {
            value = null;
            return false;
        }

        _cache.TryAdd(key, kv.Value);
        value = kv.Value;
        return true;
    }

    public OneOf<Success, NotFound> RemoveByKey(string key)
    {
        if (!IsKeyInCache(key))
        {
            return new NotFound();
        }

        _cache.Remove(key);
        RemoveKeyValueFromPersistance(key);

        return new Success();
    }

    private void RemoveKeyValueFromPersistance(string key)
    {
        var toDelete = _db.KeyValues.FirstOrDefault(kv => kv.Key == key);
        if (toDelete is null)
        {
            // FIXME: Return an error/handle this case
            return;
        }

        _db.KeyValues.Remove(toDelete);
        _db.SaveChanges();
    }

    public OneOf<Success, NotFound> UpdateKeyValue(string key, string newValue)
    {
        if (_cache.ContainsKey(key))
        {
            _cache[key] = newValue;
            return new Success();
        }

        return new NotFound();
    }

    private bool IsKeyInCache(string key) => _cache.ContainsKey(key);

    public IReadOnlyDictionary<string, string> KeyValueCache { get => _cache.KeyValues; }
}
