using KVS.Errors;
using KVS.Messages;
using KVS.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OneOf.Types;

namespace KVS.UnitTests;

public sealed class KeyValueRepositoryTests
{
    [Test]
    public async Task AddKeyValueAsync_ReturnsSuccess_WhenKeyIsNotAlreadyPresent()
    {
        // Arrange
        const string validKey = "Valid";
        const string expectedValue = "Value";

        var databaseMock = new Mock<IKeyValueDatabase>();
        databaseMock
            .Setup(m => m.AddAsync(validKey, expectedValue))
            .Verifiable();

        var busMock = new Mock<IBus>();
        busMock
            .Setup(m => m.Publish(new KeyModified(KeyValueRepository.NodeId, validKey), default))
            .Verifiable();

        var keyValueRepository = new KeyValueRepository(Logger, new KeyValueCache(), databaseMock.Object, busMock.Object);

        // Act
        var result = await keyValueRepository.AddKeyValueAsync(validKey, expectedValue);

        // Assert
        var success = result.Value as Success?;
        Assert.That(success, Is.Not.Null);

        AssertRepositoryHasKeyValuePair(keyValueRepository, validKey, expectedValue);
        databaseMock.Verify();
        busMock.Verify();
    }

    [Test]
    public async Task AddKeyValueAsync_ReturnsAlreadyPresent_WhenSameKeyIsAlreadyPresent()
    {
        // Arrange
        const string duplicateKey = "Duplicate";

        var keyValueCache = new KeyValueCache(new Dictionary<string, string>() { { duplicateKey, "" } });

        var keyValueRepository = new KeyValueRepository(Logger, keyValueCache, EmptyDb, EmptyBus);

        // Act
        var result = await keyValueRepository.AddKeyValueAsync(duplicateKey, "");

        // Assert
        var alreadyPresentError = result.Value as AlreadyPresentError?;
        Assert.That(alreadyPresentError, Is.Not.Null);
    }

    [Test]
    public async Task GetValueByKeyAsync_ReturnsSuccess_WithExpectedKey_WhenKeyIsPresent_InTheCache()
    {
        // Arrange
        const string presentKey = "present";
        const string expectedValue = "Value";

        var keyValueCache = new KeyValueCache(new Dictionary<string, string>() { { presentKey, expectedValue } });

        var keyValueRepository = new KeyValueRepository(Logger, keyValueCache, EmptyDb, EmptyBus);

        // Act
        var result = await keyValueRepository.GetValueByKeyAsync(presentKey);

        // Assert
        var success = result.Value as Success<string>?;
        Assert.That(success, Is.Not.Null);

        var actualValue = success.Value.Value;
        Assert.That(actualValue, Is.EqualTo(expectedValue));
    }

    [Test]
    public async Task GetValueByKeyAsync_ReturnsSuccess_WithExpectedKey_WhenKeyIsPresent_InTheDatabase()
    {
        // Arrange
        const string presentKey = "present";
        string? expectedValue = "Value";

        var databaseMock = new Mock<IKeyValueDatabase>();
        databaseMock
            .Setup(m => m.TryGetAsync(presentKey))
            .ReturnsAsync((true, expectedValue));

        var keyValueRepository = new KeyValueRepository(Logger, new KeyValueCache(), databaseMock.Object, EmptyBus);

        keyValueRepository.SetCacheFlagToModified(presentKey);

        // Act
        var result = await keyValueRepository.GetValueByKeyAsync(presentKey);

        // Assert
        var success = result.Value as Success<string>?;
        Assert.That(success, Is.Not.Null);

        var actualValue = success.Value.Value;
        Assert.That(actualValue, Is.EqualTo(expectedValue));
    }

    [Test]
    public async Task GetValueByKeyAsync_ReturnsNotFound_WhenKeyIsNotPresent()
    {
        // Arrange
        const string notPresentKey = "notpresent";

        var databaseMock = new Mock<IKeyValueDatabase>();
        databaseMock
            .Setup(m => m.TryGetAsync(notPresentKey))
            .ReturnsAsync((false, It.IsAny<string>()));

        var keyValueRepository = new KeyValueRepository(Logger, new KeyValueCache(), databaseMock.Object, EmptyBus);

        // Act
        var result = await keyValueRepository.GetValueByKeyAsync(notPresentKey);

        // Assert
        var notFound = result.Value as NotFound?;
        Assert.That(notFound, Is.Not.Null);
    }

    [Test]
    public async Task UpdateKeyValueAsync_ReturnsSuccess_WhenKeyIsPresent()
    {
        // Arrange
        const string presentKey = "present";
        const string expectedValue = "expected";

        var keyValueCache = new KeyValueCache(new Dictionary<string, string>() { { presentKey, "" } });

        var databaseMock = new Mock<IKeyValueDatabase>();
        databaseMock
            .Setup(m => m.UpdateAsync(presentKey, expectedValue))
            .Verifiable();

        var busMock = new Mock<IBus>();
        busMock
            .Setup(m => m.Publish(new KeyModified(KeyValueRepository.NodeId, presentKey), default))
            .Verifiable();

        var keyValueRepository = new KeyValueRepository(Logger, keyValueCache, databaseMock.Object, busMock.Object);

        // Act
        var result = await keyValueRepository.UpdateKeyValueAsync(presentKey, expectedValue);

        // Assert
        var success = result.Value as Success?;
        Assert.That(success, Is.Not.Null);

        AssertRepositoryHasKeyValuePair(keyValueRepository, presentKey, expectedValue);
        databaseMock.Verify();
        busMock.Verify();
    }

    [Test]
    public async Task UpdateKeyValueAsync_ReturnsNotFound_WhenKeyIsNotPresent()
    {
        // Arrange
        const string notPresentKey = "notpresent";

        var keyValueRepository = new KeyValueRepository(Logger, new KeyValueCache(), EmptyDb, EmptyBus);

        // Act
        var result = await keyValueRepository.UpdateKeyValueAsync(notPresentKey, "");

        // Assert
        var notFound = result.Value as NotFound?;
        Assert.That(notFound, Is.Not.Null);
    }

    [Test]
    public async Task RemoveByKeyAsync_ReturnsSuccess_WhenKeyIsPresent()
    {
        // Arrange
        const string presentKey = "present";

        var keyValueCache = new KeyValueCache(new Dictionary<string, string>() { { presentKey, "" } });

        var databaseMock = new Mock<IKeyValueDatabase>();
        databaseMock
            .Setup(m => m.DeleteAsync(presentKey))
            .Verifiable();

        var busMock = new Mock<IBus>();
        busMock
            .Setup(m => m.Publish(new KeyDeletion(KeyValueRepository.NodeId, presentKey), default))
            .Verifiable();

        var keyValueRepository = new KeyValueRepository(Logger, keyValueCache, databaseMock.Object, busMock.Object);

        // Act
        var result = await keyValueRepository.RemoveByKeyAsync(presentKey);

        // Assert
        var success = result.Value as Success?;
        Assert.That(success, Is.Not.Null);

        databaseMock.Verify();
        busMock.Verify();
    }

    [Test]
    public async Task RemoveByKeyAsync_ReturnsNotFound_WhenKeyIsNotPresent()
    {
        // Arrange
        const string notPresentKey = "notpresent";
        var keyValueRepository = new KeyValueRepository(Logger, new KeyValueCache(), EmptyDb, EmptyBus);

        // Act
        var result = await keyValueRepository.RemoveByKeyAsync(notPresentKey);

        // Assert
        var notFound = result.Value as NotFound?;
        Assert.That(notFound, Is.Not.Null);
    }

    static private void AssertRepositoryHasKeyValuePair(KeyValueRepository repo, string key, string expectedValue)
    {
        var hasKey = repo.KeyValueCache.ContainsKey(key);
        Assert.That(hasKey, Is.True);
        Assert.That(repo.KeyValueCache[key], Is.EqualTo(expectedValue));
    }

    static private NullLogger<KeyValueRepository> Logger => new();
    static private IKeyValueDatabase EmptyDb => new Mock<IKeyValueDatabase>().Object;
    static private IBus EmptyBus => new Mock<IBus>().Object;
}