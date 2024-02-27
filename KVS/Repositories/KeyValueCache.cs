using KVS.Data;
using System.Diagnostics.CodeAnalysis;

namespace KVS.Repositories;

// NOTE: This is needed as we don't want KeyValueRepository to be a singleton
public sealed class KeyValueCache : IKeyValueCache
{
    public KeyValueCache(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        InitializeCacheFromDatabase(database);
    }

    private void InitializeCacheFromDatabase(DatabaseContext database)
    {
        var keyValues = database.KeyValues.ToList();
        foreach (var kv in keyValues)
        {
            Add(kv.Key, kv.Value); 
        }
    }

    // NOTE: These constructors are used for tests
    public KeyValueCache() { }
    public KeyValueCache(Dictionary<string, string> initialKeyValues) => keyValues = initialKeyValues.ToDictionary();

    public void Add(string key, string value)
    {
        keyValues.Add(key, value);
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
