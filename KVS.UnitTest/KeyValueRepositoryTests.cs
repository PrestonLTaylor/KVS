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

        var hasKey = keyValueRepository.KeyValueCache.ContainsKey(validKey);
        Assert.That(hasKey, Is.True);
        Assert.That(keyValueRepository.KeyValueCache[validKey], Is.EqualTo(expectedValue));
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
}