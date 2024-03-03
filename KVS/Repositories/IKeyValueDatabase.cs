namespace KVS.Repositories;

public interface IKeyValueDatabase
{
    public Task AddAsync(string key, string value);
    public Task<(bool, string?)> TryGetAsync(string key);
    public Task UpdateAsync(string key, string value);
    public Task DeleteAsync(string key);
}
