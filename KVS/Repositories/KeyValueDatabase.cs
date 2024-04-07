using KVS.Data;
using Microsoft.EntityFrameworkCore;

namespace KVS.Repositories;

/// <summary>
/// Our implementation for a key value pair persistance database.
/// </summary>
/// <param name="_db">The database context for the underlying database</param>
public sealed class KeyValueDatabase(DatabaseContext _db) : IKeyValueDatabase
{
    /// <inheritdoc/>
    public async Task AddAsync(string key, string value)
    {
        // FIXME: Check if the key value was actually added and return an error
        await _db.KeyValues.AddAsync(new() { Key = key, Value = value });
        await _db.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string key)
    {
        await _db.KeyValues
            .Where(kv => kv.Key == key)
            .ExecuteDeleteAsync();
    }

    /// <inheritdoc/>
    public async Task<(bool, string?)> TryGetAsync(string key)
    {
        var kv = await _db.KeyValues.FirstOrDefaultAsync(kv => kv.Key == key);
        if (kv is null)
        {
            return (false, null);
        }

        return (true, kv.Value);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(string key, string value)
    {
        await _db.KeyValues
            .Where(kv => kv.Key == key)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(kv => kv.Value, value)
            );
    }
}
