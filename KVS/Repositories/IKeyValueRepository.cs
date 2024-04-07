using KVS.Errors;
using OneOf;
using OneOf.Types;

namespace KVS.Repositories;

/// <summary>
/// An interface that defines a contract for a key value pair repository.<br/>
/// The repository manages both the key value cache and database.<br/>
/// </summary>
public interface IKeyValueRepository
{
    /// <summary>
    /// Tries to add a key value pair to the managed cache and database.
    /// </summary>
    /// <param name="key">The key to add.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>
    /// <see cref="Success"/> if the key does not already exist.
    /// <see cref="AlreadyPresentError"/> if the key already exists.
    /// </returns>
    public Task<OneOf<Success, AlreadyPresentError>> AddKeyValueAsync(string key, string value);

    /// <summary>
    /// Tries to get a key value pair to the managed cache or database.
    /// </summary>
    /// <param name="key">The key to of a key value pair to get.</param>
    /// <returns>
    /// <see cref="Success"/> if the key exist and holds the respective value.<br/>
    /// <see cref="NotFound"/> if the key does not exist.<br/>
    /// </returns>
    public Task<OneOf<Success<string>, NotFound>> GetValueByKeyAsync(string key);

    /// <summary>
    /// Tries to update a key value pair to the managed cache or database.
    /// </summary>
    /// <param name="key">The key to of a key value pair to update.</param>
    /// <param name="newValue">The value to update to.</param>
    /// <returns>
    /// <see cref="Success"/> if the key exist and value was successfully updated.<br/>
    /// <see cref="NotFound"/> if the key does not exist.<br/>
    /// </returns>
    public Task<OneOf<Success, NotFound>> UpdateKeyValueAsync(string key, string newValue);

    /// <summary>
    /// Tries to delete a key value pair to the managed cache or database.
    /// </summary>
    /// <param name="key">The key to of a key value pair to delete.</param>
    /// <returns>
    /// <see cref="Success"/> if the key exist and value was successfully deleted.<br/>
    /// <see cref="NotFound"/> if the key does not exist.<br/>
    /// </returns>
    public Task<OneOf<Success, NotFound>> RemoveByKeyAsync(string key);

    /// <summary>
    /// Indicates to the managed cache that the passed key has been modified and should be updated.
    /// </summary>
    /// <param name="modifiedKey">The key of a key value pair that was modified.</param>
    public void SetCacheFlagToModified(string modifiedKey);

    /// <summary>
    /// Indicates to the managed cache that the passed key has been deleted and should be removed.
    /// </summary>
    /// <param name="deletedKey">The key of a key value pair that was deleted.</param>
    public void SetCacheFlagToDeleted(string deletedKey);
}
