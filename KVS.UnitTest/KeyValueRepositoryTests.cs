using KVS.Errors;
using KVS.Repositories;
using OneOf.Types;

namespace KVS.UnitTests;

public sealed class KeyValueRepositoryTests
{
    [Test]
    public void AddKeyValue_ReturnsSuccess_WhenKeyIsNotAlreadyPresent()
    {
        // Arrange
        const string validKey = "Valid";
        const string expectedValue = "Value";
        var keyValueRepository = new KeyValueRepository();

        // Act
        var result = keyValueRepository.AddKeyValue(validKey, expectedValue);

        // Assert
        var success = result.Value as Success?;
        Assert.That(success, Is.Not.Null);

        AssertRepositoryHasKeyValuePair(keyValueRepository, validKey, expectedValue);
    }

    [Test]
    public void AddKeyValue_ReturnsAlreadyPresent_WhenSameKeyIsAlreadyPresent()
    {
        // Arrange
        const string duplicateKey = "Duplicate";
        var keyValueRepository = new KeyValueRepository(new() { { duplicateKey, "" } });

        // Act
        var result = keyValueRepository.AddKeyValue(duplicateKey, "");

        // Assert
        var alreadyPresentError = result.Value as AlreadyPresentError?;
        Assert.That(alreadyPresentError, Is.Not.Null);
    }

    [Test]
    public void GetValueByKey_ReturnsSuccess_WithExpectedKey_WhenKeyIsPresent()
    {
        // Arrange
        const string presentKey = "present";
        const string expectedValue = "Value";
        var keyValueRepository = new KeyValueRepository(new() { { presentKey, expectedValue } });

        // Act
        var result = keyValueRepository.GetValueByKey(presentKey);

        // Assert
        var success = result.Value as Success<string>?;
        Assert.That(success, Is.Not.Null);

        var actualValue = success.Value.Value;
        Assert.That(actualValue, Is.EqualTo(expectedValue));
    }

    [Test]
    public void GetValueByKey_ReturnsNotFound_WhenKeyIsNotPresent()
    {
        // Arrange
        const string notPresentKey = "notpresent";
        var keyValueRepository = new KeyValueRepository();

        // Act
        var result = keyValueRepository.GetValueByKey(notPresentKey);

        // Assert
        var notFound = result.Value as NotFound?;
        Assert.That(notFound, Is.Not.Null);
    }

    [Test]
    public void UpdateKeyValue_ReturnsSuccess_WhenKeyIsPresent()
    {
        // Arrange
        const string presentKey = "present";
        const string expectedValue = "expected";
        var keyValueRepository = new KeyValueRepository(new() { { presentKey, "" } });

        // Act
        var result = keyValueRepository.UpdateKeyValue(presentKey, expectedValue);

        // Assert
        var success = result.Value as Success?;
        Assert.That(success, Is.Not.Null);

        AssertRepositoryHasKeyValuePair(keyValueRepository, presentKey, expectedValue);
    }

    [Test]
    public void UpdateKeyValue_ReturnsNotFound_WhenKeyIsNotPresent()
    {
        // Arrange
        const string notPresentKey = "notpresent";
        var keyValueRepository = new KeyValueRepository();

        // Act
        var result = keyValueRepository.UpdateKeyValue(notPresentKey, "");

        // Assert
        var notFound = result.Value as NotFound?;
        Assert.That(notFound, Is.Not.Null);
    }

    [Test]
    public void RemoveByKey_ReturnsSuccess_WhenKeyIsPresent()
    {
        // Arrange
        const string presentKey = "present";
        var keyValueRepository = new KeyValueRepository(new() { { presentKey, "" } });

        // Act
        var result = keyValueRepository.RemoveByKey(presentKey);

        // Assert
        var success = result.Value as Success?;
        Assert.That(success, Is.Not.Null);
    }

    [Test]
    public void RemoveByKey_ReturnsNotFound_WhenKeyIsNotPresent()
    {
        // Arrange
        const string notPresentKey = "notpresent";
        var keyValueRepository = new KeyValueRepository();

        // Act
        var result = keyValueRepository.RemoveByKey(notPresentKey);

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
}