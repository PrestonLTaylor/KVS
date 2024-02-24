using KVS.Errors;
using OneOf;
using OneOf.Types;

namespace KVS.Repositories;

public sealed class KeyValueRepository : IKeyValueRepository
{
    public KeyValueRepository() { }
    public KeyValueRepository(Dictionary<string, string> initialKeyValues) => keyValueCache = initialKeyValues.ToDictionary();

    public OneOf<Success, AlreadyPresentError> AddKeyValue(string key, string value)
    {
        if (keyValueCache.TryAdd(key, value))
        {
            return new Success();
        }

        return new AlreadyPresentError();
    }

    private readonly Dictionary<string, string> keyValueCache = [];
    public IReadOnlyDictionary<string, string> KeyValueCache
    {
        get => keyValueCache.AsReadOnly();
    }
}
