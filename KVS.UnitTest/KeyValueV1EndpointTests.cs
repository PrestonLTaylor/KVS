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
    public async Task HandleCreateKeyValueRequestAsync_ReturnsCreated_WhenKeyIsNotAlreadyPresent()
    {
        // Arrange
        const string notPresentKey = "notpresent";
        const string expectedValue = "value";
        var request = new CreateKeyValueRequest(notPresentKey, expectedValue);

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        keyValueRepoMock
            .Setup(m => m.AddKeyValueAsync(notPresentKey, expectedValue))
            .ReturnsAsync(new Success());

        // Act
        var result = await KeyValueEndpoints.HandleCreateKeyValueRequestAsync(CreateLogger, keyValueRepoMock.Object, request);

        // Assert
        var created = result as Created<string>;
        Assert.That(created, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(created.Location, Is.EqualTo($"/v1/{notPresentKey}"));
            Assert.That(created.Value, Is.EqualTo(expectedValue));
        });
    }

    [Test]
    public async Task HandleCreateKeyValueRequestAsync_ReturnsConflict_WhenKeyAlreadyExists()
    {
        // Arrange
        const string presentKey = "present";
        var request = new CreateKeyValueRequest(presentKey, "");

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        keyValueRepoMock
            .Setup(m => m.AddKeyValueAsync(presentKey, It.IsAny<string>()))
            .ReturnsAsync(new AlreadyPresentError());

        // Act
        var result = await KeyValueEndpoints.HandleCreateKeyValueRequestAsync(CreateLogger, keyValueRepoMock.Object, request);

        // Assert
        var created = result as Conflict<string>;
        Assert.That(created, Is.Not.Null);
    }

    [Test]
    public async Task HandleCreateKeyValueRequestAsync_ReturnsBadRequest_WhenKeyIsNull()
    {
        // Arrange
        var request = new CreateKeyValueRequest(null!, "");

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        // Act
        var result = await KeyValueEndpoints.HandleCreateKeyValueRequestAsync(CreateLogger, keyValueRepoMock.Object, request);

        // Assert
        var badRequest = result as BadRequest<string>;
        Assert.That(badRequest, Is.Not.Null);
    }

    [Test]
    public async Task HandleCreateKeyValueRequestAsync_ReturnsBadRequest_WhenValueIsNull()
    {
        // Arrange
        var request = new CreateKeyValueRequest("", null!); 

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        // Act
        var result = await KeyValueEndpoints.HandleCreateKeyValueRequestAsync(CreateLogger, keyValueRepoMock.Object, request);

        // Assert
        var badRequest = result as BadRequest<string>;
        Assert.That(badRequest, Is.Not.Null);
    }

    [Test]
    public async Task HandleReadValueRequestAsync_ReturnsOk_WhenKeyIsPresent()
    {
        // Arrange
        const string presentKey = "present";
        const string expectedValue = "value";

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        keyValueRepoMock
            .Setup(m => m.GetValueByKeyAsync(presentKey))
            .ReturnsAsync(new Success<string>(expectedValue));

        // Act
        var result = await KeyValueEndpoints.HandleReadValueRequestAsync(ReadLogger, keyValueRepoMock.Object, presentKey);

        // Assert
        var ok = result as Ok<string>;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok.Value, Is.EqualTo(expectedValue));
    }

    [Test]
    public async Task HandleReadValueRequestAsync_ReturnsNotFound_WhenKeyIsNotPresent()
    {
        // Arrange
        const string notPresentKey = "notpresent";

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        keyValueRepoMock
            .Setup(m => m.GetValueByKeyAsync(notPresentKey))
            .ReturnsAsync(new OneOf.Types.NotFound());

        // Act
        var result = await KeyValueEndpoints.HandleReadValueRequestAsync(ReadLogger, keyValueRepoMock.Object, notPresentKey);

        // Assert
        var notFound = result as NotFound<string>;
        Assert.That(notFound, Is.Not.Null);
    }

    [Test]
    public async Task HandleReadValueRequestAsync_ReturnsBadRequest_WhenKeyIsNull()
    {
        // Arrange
        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        // Act
        var result = await KeyValueEndpoints.HandleReadValueRequestAsync(ReadLogger, keyValueRepoMock.Object, null!);

        // Assert
        var badRequest = result as BadRequest<string>;
        Assert.That(badRequest, Is.Not.Null);
    }

    [Test]
    public async Task HandleUpdateKeyValueRequestAsync_ReturnsNoContent_WhenKeyIsPresent()
    {
        // Arrange
        const string presentKey = "present";
        var request = new UpdateKeyValueRequest(presentKey, "");

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        keyValueRepoMock
            .Setup(m => m.UpdateKeyValueAsync(presentKey, It.IsAny<string>()))
            .ReturnsAsync(new Success());

        // Act
        var result = await KeyValueEndpoints.HandleUpdateKeyValueRequestAsync(UpdateLogger, keyValueRepoMock.Object, request);

        // Assert
        var noContent = result as NoContent;
        Assert.That(noContent, Is.Not.Null);
    }

    [Test]
    public async Task HandleUpdateKeyValueRequestAsync_ReturnsNotFound_WhenKeyIsNotPresent()
    {
        // Arrange
        const string notPresentKey = "notpresent";
        var request = new UpdateKeyValueRequest(notPresentKey, "");

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        keyValueRepoMock
            .Setup(m => m.UpdateKeyValueAsync(notPresentKey, It.IsAny<string>()))
            .ReturnsAsync(new OneOf.Types.NotFound());

        // Act
        var result = await KeyValueEndpoints.HandleUpdateKeyValueRequestAsync(UpdateLogger, keyValueRepoMock.Object, request);

        // Assert
        var notFound = result as NotFound<string>;
        Assert.That(notFound, Is.Not.Null);
    }

    [Test]
    public async Task HandleUpdateKeyValueRequestAsync_ReturnsBadRequest_WhenKeyIsNull()
    {
        // Arrange
        var request = new UpdateKeyValueRequest(null!, "");

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        // Act
        var result = await KeyValueEndpoints.HandleUpdateKeyValueRequestAsync(UpdateLogger, keyValueRepoMock.Object, request);

        // Assert
        var badRequest = result as BadRequest<string>;
        Assert.That(badRequest, Is.Not.Null);
    }

    [Test]
    public async Task HandleUpdateKeyValueRequestAsync_ReturnsBadRequest_WhenValueIsNull()
    {
        // Arrange
        var request = new UpdateKeyValueRequest("", null!);

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        // Act
        var result = await KeyValueEndpoints.HandleUpdateKeyValueRequestAsync(UpdateLogger, keyValueRepoMock.Object, request);

        // Assert
        var badRequest = result as BadRequest<string>;
        Assert.That(badRequest, Is.Not.Null);
    }

    [Test]
    public async Task HandleRemoveKeyRequestAsync_ReturnsNoContent_WhenKeyIsPresent()
    {
        // Arrange
        const string presentKey = "present";
        var request = new RemoveKeyRequest(presentKey);

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        keyValueRepoMock
            .Setup(m => m.RemoveByKeyAsync(presentKey))
            .ReturnsAsync(new Success());

        // Act
        var result = await KeyValueEndpoints.HandleRemoveKeyRequestAsync(DeleteLogger, keyValueRepoMock.Object, request);

        // Assert
        var noContent = result as NoContent;
        Assert.That(noContent, Is.Not.Null);
    }

    [Test]
    public async Task HandleRemoveKeyRequestAsync_ReturnsNotFound_WhenKeyIsNotPresent()
    {
        // Arrange
        const string notPresentKey = "notpresent";
        var request = new RemoveKeyRequest(notPresentKey);

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        keyValueRepoMock
            .Setup(m => m.RemoveByKeyAsync(notPresentKey))
            .ReturnsAsync(new OneOf.Types.NotFound());

        // Act
        var result = await KeyValueEndpoints.HandleRemoveKeyRequestAsync(DeleteLogger, keyValueRepoMock.Object, request);

        // Assert
        var notFound = result as NotFound<string>;
        Assert.That(notFound, Is.Not.Null);
    }

    [Test]
    public async Task HandleRemoveKeyRequestAsync_ReturnsBadRequest_WhenKeyIsNull()
    {
        // Arrange
        var request = new RemoveKeyRequest(null!);

        var keyValueRepoMock = new Mock<IKeyValueRepository>();

        // Act
        var result = await KeyValueEndpoints.HandleRemoveKeyRequestAsync(DeleteLogger, keyValueRepoMock.Object, request);

        // Assert
        var badRequest = result as BadRequest<string>;
        Assert.That(badRequest, Is.Not.Null);
    }

    static private NullLogger<CreateKeyValueRequest> CreateLogger { get; } = new();
    static private NullLogger<ReadValueRequest> ReadLogger { get; } = new();
    static private NullLogger<UpdateKeyValueRequest> UpdateLogger { get; } = new();
    static private NullLogger<RemoveKeyRequest> DeleteLogger { get; } = new();
}
