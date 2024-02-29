using System.Diagnostics.CodeAnalysis;

namespace KVS.Repositories;

public interface IKeyValueDatabase
{
    public void Add(string key, string value);
    public bool TryGet(string key, [MaybeNullWhen(false)] out string value);
    public void Update(string key, string value);
    public void Delete(string key);
}
