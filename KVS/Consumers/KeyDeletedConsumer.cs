using KVS.Messages;
using KVS.Repositories;
using MassTransit;

namespace KVS.Consumers;

public sealed class KeyDeletedConsumer(ILogger<KeyDeletedConsumer> _logger, IKeyValueRepository _repo) : IConsumer<KeyDeletion>
{
    public Task Consume(ConsumeContext<KeyDeletion> context)
    {
        // Filter messages sent from our own node
        if (context.Message.NodeId != KeyValueRepository.NodeId)
        {
            var deletedKey = context.Message.DeletedKey;
            _logger.LogInformation("Recieved a KeyDeletion message for the key '{DeletedKey}'", deletedKey);

            _repo.SetCacheFlagToDeleted(deletedKey);
        }

        return Task.CompletedTask;
    }
}

