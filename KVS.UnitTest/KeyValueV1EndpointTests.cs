using KVS.Endpoints.v1;
using KVS.Errors;
using KVS.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OneOf.Types;

namespace KVS.UnitTests;

public sealed class KeyValueV1EndpointTests
{
    [Test]
    public void HandleCreateKeyValueRequest_ReturnsCreated_WhenKeyIsNotAlreadyPresent()
    {
        // Arrange
        const string notPresentKey = "notpresent";
        const string expectedValue = "value";
        var request = new CreateKeyValueRequest(notPresentKey, expectedValue);

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        keyValueRepoMock
            .Setup(m => m.AddKeyValue(notPresentKey, expectedValue))
            .Returns(new Success());

        // Act
        var result = KeyValueEndpoints.HandleCreateKeyValueRequest(CreateLogger, keyValueRepoMock.Object, request);

        // Assert
        var created = result as Created<string>;
        Assert.That(created, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(created.Location, Is.EqualTo($"/{notPresentKey}"));
            Assert.That(created.Value, Is.EqualTo(expectedValue));
        });
    }

    [Test]
    public void HandleCreateKeyValueRequest_ReturnsConflict_WhenKeyAlreadyExists()
    {
        // Arrange
        const string presentKey = "present";
        var request = new CreateKeyValueRequest(presentKey, "");

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        keyValueRepoMock
            .Setup(m => m.AddKeyValue(presentKey, It.IsAny<string>()))
            .Returns(new AlreadyPresentError());

        // Act
        var result = KeyValueEndpoints.HandleCreateKeyValueRequest(CreateLogger, keyValueRepoMock.Object, request);

        // Assert
        var created = result as Conflict<string>;
        Assert.That(created, Is.Not.Null);
    }

    [Test]
    public void HandleReadValueRequest_ReturnsOk_WhenKeyIsPresent()
    {
        // Arrange
        const string presentKey = "present";
        const string expectedValue = "value";

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        keyValueRepoMock
            .Setup(m => m.GetValueByKey(presentKey))
            .Returns(new Success<string>(expectedValue));

        // Act
        var result = KeyValueEndpoints.HandleReadValueRequest(ReadLogger, keyValueRepoMock.Object, presentKey);

        // Assert
        var ok = result as Ok<string>;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok.Value, Is.EqualTo(expectedValue));
    }

    [Test]
    public void HandleReadValueRequest_ReturnsNotFound_WhenKeyIsNotPresent()
    {
        // Arrange
        const string notPresentKey = "notpresent";

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        keyValueRepoMock
            .Setup(m => m.GetValueByKey(notPresentKey))
            .Returns(new OneOf.Types.NotFound());

        // Act
        var result = KeyValueEndpoints.HandleReadValueRequest(ReadLogger, keyValueRepoMock.Object, notPresentKey);

        // Assert
        var notFound = result as NotFound<string>;
        Assert.That(notFound, Is.Not.Null);
    }

    [Test]
    public void HandleUpdateKeyValueRequest_ReturnsNoContent_WhenKeyIsPresent()
    {
        // Arrange
        const string presentKey = "present";
        var request = new UpdateKeyValueRequest(presentKey, "");

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        keyValueRepoMock
            .Setup(m => m.UpdateKeyValue(presentKey, It.IsAny<string>()))
            .Returns(new Success());

        // Act
        var result = KeyValueEndpoints.HandleUpdateKeyValueRequest(UpdateLogger, keyValueRepoMock.Object, request);

        // Assert
        var noContent = result as NoContent;
        Assert.That(noContent, Is.Not.Null);
    }

    [Test]
    public void HandleUpdateKeyValueRequest_ReturnsNotFound_WhenKeyIsNotPresent()
    {
        // Arrange
        const string notPresentKey = "notpresent";
        var request = new UpdateKeyValueRequest(notPresentKey, "");

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        keyValueRepoMock
            .Setup(m => m.UpdateKeyValue(notPresentKey, It.IsAny<string>()))
            .Returns(new OneOf.Types.NotFound());

        // Act
        var result = KeyValueEndpoints.HandleUpdateKeyValueRequest(UpdateLogger, keyValueRepoMock.Object, request);

        // Assert
        var notFound = result as NotFound<string>;
        Assert.That(notFound, Is.Not.Null);
    }

    [Test]
    public void HandleRemoveKeyRequest_ReturnsNoContent_WhenKeyIsPresent()
    {
        // Arrange
        const string presentKey = "present";
        var request = new RemoveKeyRequest(presentKey);

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        keyValueRepoMock
            .Setup(m => m.RemoveByKey(presentKey))
            .Returns(new Success());

        // Act
        var result = KeyValueEndpoints.HandleRemoveKeyRequest(DeleteLogger, keyValueRepoMock.Object, request);

        // Assert
        var noContent = result as NoContent;
        Assert.That(noContent, Is.Not.Null);
    }

    [Test]
    public void HandleRemoveKeyRequest_ReturnsNotFound_WhenKeyIsNotPresent()
    {
        // Arrange
        const string notPresentKey = "notpresent";
        var request = new RemoveKeyRequest(notPresentKey);

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        keyValueRepoMock
            .Setup(m => m.RemoveByKey(notPresentKey))
            .Returns(new OneOf.Types.NotFound());

        // Act
        var result = KeyValueEndpoints.HandleRemoveKeyRequest(DeleteLogger, keyValueRepoMock.Object, request);

        // Assert
        var notFound = result as NotFound<string>;
        Assert.That(notFound, Is.Not.Null);
    }

    static private NullLogger<CreateKeyValueRequest> CreateLogger { get; } = new();
    static private NullLogger<ReadValueRequest> ReadLogger { get; } = new();
    static private NullLogger<UpdateKeyValueRequest> UpdateLogger { get; } = new();
    static private NullLogger<RemoveKeyRequest> DeleteLogger { get; } = new();
}
