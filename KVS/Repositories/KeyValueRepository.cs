using KVS.Errors;
using OneOf;
using OneOf.Types;

namespace KVS.Repositories;

public sealed class KeyValueRepository(IKeyValueCache _cache) : IKeyValueRepository
{
    public OneOf<Success, AlreadyPresentError> AddKeyValue(string key, string value)
    {
        if (_cache.TryAdd(key, value))
        {
            return new Success();
        }

        return new AlreadyPresentError();
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

    public IReadOnlyDictionary<string, string> KeyValueCache { get => _cache.KeyValues; }
}
