using KVS.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace KVS.Repositories;

// FIXME: Testing for this class (integration tests) when we have pubsub messaging set up
public sealed class KeyValueDatabase(DatabaseContext _db) : IKeyValueDatabase
{
    public void Add(string key, string value)
    {
        // FIXME: Check if the key value was actually added and return an error
        // FIXME: Convert functions to async functions
        _db.KeyValues.Add(new() { Key = key, Value = value });
        _db.SaveChanges();
    }

    public void Delete(string key)
    {
        _db.KeyValues
            .Where(kv => kv.Key == key)
            .ExecuteDelete();
    }

    public bool TryGet(string key, [MaybeNullWhen(false)] out string value)
    {
        var kv = _db.KeyValues.FirstOrDefault(kv => kv.Key == key);
        if (kv is null)
        {
            value = null;
            return false;
        }

        value = kv.Value;
        return true;
    }

    public void Update(string key, string value)
    {
        _db.KeyValues
            .Where(kv => kv.Key == key)
            .ExecuteUpdate(
                setters => setters.SetProperty(kv => kv.Value, value)
            );
    }
}
