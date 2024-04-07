using KVS.Messages;
using KVS.Repositories;
using MassTransit;

namespace KVS.Consumers;

/// <summary>
/// Our message handler for consuming <see cref="KeyModified"/> messages from other nodes.<br/>
/// When a key value pair is modified in another node, that node sents a KeyModified message.<br/>
/// Every other node updates the value in the cache from persistance to keep up to date.<br/>
/// </summary>
/// <param name="_logger">The logger for <see cref="KeyModifiedConsumer"/>.</param>
/// <param name="_repo">The repository that handles the persistance and caching of key value pairs.</param>
public sealed class KeyModifiedConsumer(ILogger<KeyModifiedConsumer> _logger, IKeyValueRepository _repo) : IConsumer<KeyModified>
{
    /// <summary>
    /// Consumes a <see cref="KeyModified"/> message being sent from other nodes.
    /// Makes sure that our cache is up to date for the key value that was modified from another node.
    /// </summary>
    /// <param name="context">The context of the message being sent from other nodes</param>
    public Task Consume(ConsumeContext<KeyModified> context)
    {
        // Filter messages sent from our own node
        if (context.Message.NodeId != KeyValueRepository.NodeId)
        {
            var modifiedKey = context.Message.ModifiedKey;
            _logger.LogInformation("Recieved a KeyModified message for the key '{ModifiedKey}'", modifiedKey);

            _repo.SetCacheFlagToModified(modifiedKey);
        }

        return Task.CompletedTask;
    }
}
