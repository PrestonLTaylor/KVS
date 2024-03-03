using KVS.Consumers;
using KVS.Messages;
using KVS.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace KVS.UnitTests;

internal sealed class MessageConsumerTests
{
    [Test]
    public async Task KeyModified_Consume_SetsCacheFlagToModified_ForProvidedKey()
    {
        // Arrange
        const string modifiedKey = "modified";

        var repoMock = new Mock<IKeyValueRepository>();
        repoMock
            .Setup(m => m.SetCacheFlagToModified(modifiedKey))
            .Verifiable();

        var keyModifiedConsumer = new KeyModifiedConsumer(NullLogger<KeyModifiedConsumer>.Instance, repoMock.Object);

        var contextMock = new Mock<ConsumeContext<KeyModified>>();
        contextMock
            .SetupGet(m => m.Message)
            .Returns(new KeyModified(modifiedKey));
        
        // Act
        await keyModifiedConsumer.Consume(contextMock.Object);

        // Assert
        repoMock.Verify();
    }

    [Test]
    public async Task KeyDeletion_Consume_SetsCacheFlagToDeleted_ForProvidedKey()
    {
        // Arrange
        const string deletedKey = "modified";

        var repoMock = new Mock<IKeyValueRepository>();
        repoMock
            .Setup(m => m.SetCacheFlagToDeleted(deletedKey))
            .Verifiable();

        var keyModifiedConsumer = new KeyDeletedConsumer(NullLogger<KeyDeletedConsumer>.Instance, repoMock.Object);

        var contextMock = new Mock<ConsumeContext<KeyDeletion>>();
        contextMock
            .SetupGet(m => m.Message)
            .Returns(new KeyDeletion(deletedKey));

        // Act
        await keyModifiedConsumer.Consume(contextMock.Object);

        // Assert
        repoMock.Verify();
    }
}
