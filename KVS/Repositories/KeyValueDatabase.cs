using KVS.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace KVS.Repositories;

// FIXME: Testing for this class (integration tests) when we have pubsub messaging set up
public sealed class KeyValueDatabase(DatabaseContext _db) : IKeyValueDatabase
{
    public async Task AddAsync(string key, string value)
    {
        // FIXME: Check if the key value was actually added and return an error
        await _db.KeyValues.AddAsync(new() { Key = key, Value = value });
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string key)
    {
        await _db.KeyValues
            .Where(kv => kv.Key == key)
            .ExecuteDeleteAsync();
    }

    public async Task<(bool, string?)> TryGetAsync(string key)
    {
        var kv = await _db.KeyValues.FirstOrDefaultAsync(kv => kv.Key == key);
        if (kv is null)
        {
            return (false, null);
        }

        return (true, kv.Value);
    }

    public async Task UpdateAsync(string key, string value)
    {
        await _db.KeyValues
            .Where(kv => kv.Key == key)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(kv => kv.Value, value)
            );
    }
}
