using KVS.Errors;
using OneOf;
using OneOf.Types;

namespace KVS.Repositories;

public interface IKeyValueRepository
{
    public Task<OneOf<Success, AlreadyPresentError>> AddKeyValueAsync(string key, string value);
    public Task<OneOf<Success<string>, NotFound>> GetValueByKeyAsync(string key);
    public Task<OneOf<Success, NotFound>> UpdateKeyValueAsync(string key, string newValue);
    public Task<OneOf<Success, NotFound>> RemoveByKeyAsync(string key);

    public void SetCacheFlagToModified(string modifiedKey);
    public void SetCacheFlagToDeleted(string deletedKey);
}
