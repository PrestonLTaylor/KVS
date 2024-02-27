using System.Diagnostics.CodeAnalysis;

namespace KVS.Repositories;

public interface IKeyValueCache
{
    void Add(string key, string value);
    bool TryGetValue(string key, [MaybeNullWhen(false)] out string value);
    bool Remove(string key);
    bool ContainsKey(string key);

    string this[string index] { get; set; }

    public IReadOnlyDictionary<string, string> KeyValues { get; }
}
