using System.Diagnostics.CodeAnalysis;

namespace KVS.Repositories;

/// <summary>
/// An interface that defines a contract for a key value pair in-memory cache.
/// </summary>
public interface IKeyValueCache
{
    /// <summary>
    /// Adds a key value pair to our in-memory cache.
    /// </summary>
    /// <param name="key">The key to add</param>
    /// <param name="value">The value to add</param>
    void Add(string key, string value);

    /// <summary>
    /// Tries to get a key value pair from our in-memory cache. <br/>
    /// If the key is not found, value is will be null. <br/>
    /// </summary>
    /// <param name="key">The key to get from the our cache</param>
    /// <param name="value">The out param that will contain the respective value for a key or null if not found.</param>
    /// <returns><see cref="true"/> if the key was found, otherwise, returns <see cref="false"/>.</returns>
    bool TryGetValue(string key, [MaybeNullWhen(false)] out string value);

    /// <summary>
    /// Tries to delete a key of a key value pair from our in-memory cache.
    /// </summary>
    /// <param name="key">The key of a key value pair to remove from our cache.</param>
    /// <returns><see cref="true"/> if the key was deleted, otherwise, returns <see cref="false"/></returns>
    bool Remove(string key);

    /// <summary>
    /// Checks if a key is contained within our in-memory key value cache.
    /// </summary>
    /// <param name="key">The key of a key value pair to check.</param>
    /// <returns><see cref="true"/> if the key was found, otherwise, returns <see cref="false"/>.</returns>
    bool ContainsKey(string key);

    /// <summary>
    /// Updates a key value pair to a new value if the key already exists. <br/>
    /// Otherwise, creates a new key value pair. <br/>
    /// </summary>
    /// <param name="key">The key to update or add.</param>
    /// <param name="value">The value to update or add.</param>
    void UpdateOrAdd(string key, string value);

    /// <summary>
    /// Indexer for retreiving the value of a key value pair by its key.
    /// </summary>
    /// <param name="index">The key of a key value pair to retrieve.</param>
    /// <returns>The respective value for the passed <see cref="index"/>.</returns>
    string this[string index] { get; set; }

    /// <summary>
    /// The read only dictionary for directly reading key value pairs from outside of the instance.
    /// </summary>
    public IReadOnlyDictionary<string, string> KeyValues { get; }
}
