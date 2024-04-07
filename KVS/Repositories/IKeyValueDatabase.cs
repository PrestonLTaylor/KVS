namespace KVS.Repositories;

/// <summary>
/// An interface that defines a contract for a key value pair persistance database.
/// </summary>
public interface IKeyValueDatabase
{
    /// <summary>
    /// Adds a key value pair to the underlying database.
    /// </summary>
    /// <param name="key">The key to add.</param>
    /// <param name="value">The value to add.</param>
    public Task AddAsync(string key, string value);

    /// <summary>
    /// Tries to get a key value pair by its key from the underlying database.
    /// </summary>
    /// <param name="key">The key to search for.</param>
    /// <returns>
    /// A tuple of a bool and a nullable string.<br/>
    /// If the key was found, the tuple contains true and the value of the found pair.<br/>
    /// If the key was not found, the tuple contains false and null.<br/>
    /// </returns>
    public Task<(bool, string?)> TryGetAsync(string key);

    /// <summary>
    /// Updates a key value pair by its key in the underlying database.
    /// </summary>
    /// <param name="key">The key of a key value pair to update.</param>
    /// <param name="value">The value to update.</param>
    public Task UpdateAsync(string key, string value);

    /// <summary>
    /// Deletes a key value pair by its key in the underlying database.
    /// </summary>
    /// <param name="key">The key of a key value pair to delete.</param>
    public Task DeleteAsync(string key);
}
