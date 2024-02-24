using KVS.Errors;
using OneOf;
using OneOf.Types;

namespace KVS.Repositories;

public interface IKeyValueRepository
{
    public OneOf<Success, AlreadyPresentError> AddKeyValue(string key, string value);
    public OneOf<Success<string>, NotFound> GetValueByKey(string key);
    public OneOf<Success, NotFound> UpdateKeyValue(string key, string newValue);
    public OneOf<Success, NotFound> RemoveByKey(string key);
}
