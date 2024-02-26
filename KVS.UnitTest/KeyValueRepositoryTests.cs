using KVS.Data;
using KVS.Errors;
using KVS.Models;
using KVS.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
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

        var dbSetMock = CreateDbSetMock(Enumerable.Empty<KeyValueModel>().AsQueryable());
        var databaseMock = CreateDatabaseContextMock(dbSetMock.Object);

        var keyValueRepository = new KeyValueRepository(new KeyValueCache(), databaseMock.Object);

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
        var keyValueCache = new KeyValueCache(new() { { duplicateKey, "" } });
        var keyValueRepository = new KeyValueRepository(keyValueCache, EmptyDb);

        // Act
        var result = keyValueRepository.AddKeyValue(duplicateKey, "");

        // Assert
        var alreadyPresentError = result.Value as AlreadyPresentError?;
        Assert.That(alreadyPresentError, Is.Not.Null);
    }

    [Test]
    public void GetValueByKey_ReturnsSuccess_WithExpectedKey_WhenKeyIsPresent_InTheCache()
    {
        // Arrange
        const string presentKey = "present";
        const string expectedValue = "Value";
        var keyValueCache = new KeyValueCache(new() { { presentKey, expectedValue } });
        var keyValueRepository = new KeyValueRepository(keyValueCache, EmptyDb);

        // Act
        var result = keyValueRepository.GetValueByKey(presentKey);

        // Assert
        var success = result.Value as Success<string>?;
        Assert.That(success, Is.Not.Null);

        var actualValue = success.Value.Value;
        Assert.That(actualValue, Is.EqualTo(expectedValue));
    }

    [Test]
    public void GetValueByKey_ReturnsSuccess_WithExpectedKey_WhenKeyIsPresent_InTheDatabase()
    {
        // Arrange
        const string presentKey = "present";
        const string expectedValue = "Value";

        var keyValueCache = new KeyValueCache();

        var databaseData = new List<KeyValueModel>()
        {
            new() { Key = presentKey, Value = expectedValue }
        };
        var dbSetMock = CreateDbSetMock(databaseData.AsQueryable());
        var databaseMock = CreateDatabaseContextMock(dbSetMock.Object);

        var keyValueRepository = new KeyValueRepository(keyValueCache, databaseMock.Object);

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

        var dbSetMock = CreateDbSetMock(Enumerable.Empty<KeyValueModel>().AsQueryable());
        var databaseMock = CreateDatabaseContextMock(dbSetMock.Object);

        var keyValueRepository = new KeyValueRepository(new KeyValueCache(), databaseMock.Object);

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
        var keyValueCache = new KeyValueCache(new() { { presentKey, "" } });
        var keyValueRepository = new KeyValueRepository(keyValueCache, EmptyDb);

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
        var keyValueRepository = new KeyValueRepository(new KeyValueCache(), EmptyDb);

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
        var keyValueCache = new KeyValueCache(new() { { presentKey, "" } });
        var keyValueRepository = new KeyValueRepository(keyValueCache, EmptyDb);

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
        var keyValueRepository = new KeyValueRepository(new KeyValueCache(), EmptyDb);

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

    static private Mock<DbSet<T>> CreateDbSetMock<T>(IQueryable<T> setData) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();

        // Sets up the queryable properties so extension methods will work on the DbSet, https://learn.microsoft.com/en-us/ef/ef6/fundamentals/testing/mocking?redirectedfrom=MSDN
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(setData.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(setData.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(setData.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => setData.GetEnumerator());

        return mockSet;
    }

    static private Mock<DatabaseContext> CreateDatabaseContextMock(DbSet<KeyValueModel> kvDbSet)
    {
        var mock = CreateEmptyDatabaseContextMock();
        mock.Setup(m => m.KeyValues).Returns(kvDbSet);
        return mock;
    }

    static private Mock<DatabaseContext> CreateEmptyDatabaseContextMock()
    {
        var logger = NullLogger<DatabaseContext>.Instance;
        var configuration = new Mock<IConfiguration>().Object;
        return new Mock<DatabaseContext>(logger, configuration);
    }

    static private DatabaseContext EmptyDb
    {
        get
        {
            return CreateEmptyDatabaseContextMock().Object;
        }
    }
}