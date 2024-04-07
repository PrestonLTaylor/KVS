using KVS.Messages;
using KVS.Repositories;
using MassTransit;

namespace KVS.Consumers;

/// <summary>
/// Our message handler for consuming <see cref="KeyDeletion"/> messages from other nodes.<br/>
/// When a key value pair is deleted in another node, that node sents a KeyDeletion message.<br/>
/// Every other node deletes the key value pair in the cache from persistance to keep up to date.<br/>
/// </summary>
/// <param name="_logger">The logger for <see cref="KeyDeletedConsumer"/>.</param>
/// <param name="_repo">The repository that handles the persistance and caching of key value pairs.</param>
public sealed class KeyDeletedConsumer(ILogger<KeyDeletedConsumer> _logger, IKeyValueRepository _repo) : IConsumer<KeyDeletion>
{
    /// <summary>
    /// Consumes a <see cref="KeyDeletion"/> message being sent from other nodes.
    /// Makes sure that our cache does not keep the key value that was deleted from another node.
    /// </summary>
    /// <param name="context">The context of the message being sent from other nodes</param>
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

