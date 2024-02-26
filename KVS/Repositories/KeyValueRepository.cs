using KVS.Data;
using KVS.Errors;
using OneOf;
using OneOf.Types;

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

        return new NotFound();
    }

    public OneOf<Success, NotFound> RemoveByKey(string key)
    {
        if (_cache.Remove(key))
        {
            return new Success();
        }

        return new NotFound();  
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
