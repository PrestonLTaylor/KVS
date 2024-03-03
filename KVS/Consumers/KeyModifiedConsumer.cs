using KVS.Messages;
using KVS.Repositories;
using MassTransit;

namespace KVS.Consumers;

public sealed class KeyModifiedConsumer(ILogger<KeyModifiedConsumer> _logger, IKeyValueRepository _repo) : IConsumer<KeyModified>
{
    public Task Consume(ConsumeContext<KeyModified> context)
    {
        var modifiedKey = context.Message.ModifiedKey;
        _logger.LogInformation("Recieved a KeyModified message for the key '{ModifiedKey}'", modifiedKey);

        _repo.SetCacheFlagToModified(modifiedKey);

        return Task.CompletedTask;
    }
}
