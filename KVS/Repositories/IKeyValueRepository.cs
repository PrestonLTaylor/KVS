using KVS.Errors;
using OneOf;
using OneOf.Types;

namespace KVS.Repositories;

public interface IKeyValueRepository
{
    public OneOf<Success, AlreadyPresentError> AddKeyValue(string key, string value);
}
