using System.Diagnostics.CodeAnalysis;

namespace KVS.Repositories;

// NOTE: This is needed as we don't want KeyValueRepository to be a singleton
public sealed class KeyValueCache : IKeyValueCache
{
    public KeyValueCache() { }
    public KeyValueCache(Dictionary<string, string> initialKeyValues) => keyValues = initialKeyValues.ToDictionary();

    public bool TryAdd(string key, string value)
    {
        return keyValues.TryAdd(key, value);
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        return keyValues.TryGetValue(key, out value);
    }

    public bool Remove(string key)
    {
        return keyValues.Remove(key);
    }
   
    public bool ContainsKey(string key)
    {
        return keyValues.ContainsKey(key);
    }

    public string this[string key] 
    { 
        get => keyValues[key];
        set => keyValues[key] = value; 
    }

    private readonly Dictionary<string, string> keyValues = [];
    public IReadOnlyDictionary<string, string> KeyValues
    {
        get => keyValues.AsReadOnly();
    }
}
