using KVS.Endpoints.v1;
using KVS.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace KVS.IntegrationTests;

internal sealed class KeyValueV1EndpointTests : KeyValueV1ApiFactory
{
    [SetUp]
    public async Task StartTestContainers()
    {
        await psqlContainer.StartAsync();
        await rabbitMqContainer.StartAsync();
    }

    [TearDown]
    public async Task DisposeTestContainers()
    {
        await psqlContainer.DisposeAsync();
        await rabbitMqContainer.DisposeAsync();
    }

    [Test]
    public async Task CreateKeyValueEndpoint_ReturnsCreated_WhenKeyDoesNotAlreadyExist()
    {
        // Arrange
        const string notPresentKey = "notpresent";
        const string expectedValue = "expected";

        var client = CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("v1/", new { NewKey = notPresentKey, Value = expectedValue });

        // Assert
        Assert.That(response, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Headers.Location!.ToString(), Is.EqualTo($"/v1/{notPresentKey}"));
        });
    }

    [Test]
    public async Task CreateKeyValueEndpoint_ReturnsBadRequest_WhenNewKeyIsNull()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("v1/", new { NewKey = (string?)null, Value = "" });

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateKeyValueEndpoint_ReturnsBadRequest_WhenValueIsNull()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("v1/", new { NewKey = "", Value = (string?)null });

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateKeyValueEndpoint_ReturnsConflict_WhenKeyAlreadyExists()
    {
        // Arrange
        const string presentKey = "present";

        var client = CreateClient();

        await CreateKeyValueInPsqlContainerAsync(presentKey, "value");

        // Act
        var response = await client.PostAsJsonAsync("v1/", new { NewKey = presentKey, Value = "" });

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task ReadKeyValueEndpoint_ReturnsOk_WhenKeyExists()
    {
        // Arrange
        const string presentKey = "present";
        const string expectedValue = "expected";

        var client = CreateClient();

        await CreateKeyValueInPsqlContainerAsync(presentKey, expectedValue);

        // Act
        var response = await client.GetAsync($"/v1/{presentKey}");

        // Assert
        Assert.That(response, Is.Not.Null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var actualValue = await response.Content.ReadAsStringAsync();
        Assert.That(actualValue, Is.EqualTo($"\"{expectedValue}\""));
    }

    [Test]
    public async Task ReadKeyValueEndpoint_ReturnsNotFound_WhenKeyDoesNotExists()
    {
        // Arrang
        const string notPresentKey = "notpresent";

        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/v1/{notPresentKey}");

        // Assert
        Assert.That(response, Is.Not.Null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task UpdateKeyValueEndpoint_ReturnsNoContent_WhenKeyExists()
    {
        // Arrange
        const string presentKey = "present";

        var client = CreateClient();

        await CreateKeyValueInPsqlContainerAsync(presentKey, "");

        // Act
        var response = await client.PutAsJsonAsync($"/v1/", new { Key = presentKey, NewValue = "value" });

        // Assert
        Assert.That(response, Is.Not.Null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task UpdateKeyValueEndpoint_ReturnsBadRequest_WhenKeyIsNull()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.PutAsJsonAsync("v1/", new { Key = (string?)null, NewValue = "" });

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task UpdateKeyValueEndpoint_ReturnsBadRequest_WhenNewValueIsNull()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.PutAsJsonAsync("v1/", new { Key = "", NewValue = (string?)null });

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task UpdateKeyValueEndpoint_ReturnsNotFound_WhenKeyDoesNotExists()
    {
        // Arrange
        const string notPresentKey = "notpresent";

        var client = CreateClient();

        // Act
        var response = await client.PutAsJsonAsync($"/v1/", new { Key = notPresentKey, NewValue = "" });

        // Assert
        Assert.That(response, Is.Not.Null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task DeleteKeyValueEndpoint_ReturnsNoContent_WhenKeyExists()
    {
        // Arrange
        const string presentKey = "present";

        var client = CreateClient();

        await CreateKeyValueInPsqlContainerAsync(presentKey, "");

        // Act
        var response = await DeleteAsJsonAsync(client, "v1/", new { Key = presentKey });

        // Assert
        Assert.That(response, Is.Not.Null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task DeleteKeyValueEndpoint_ReturnsBadRequest_WhenKeyIsNull()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await DeleteAsJsonAsync(client, "v1/", new { Key = (string?)null });

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task DeleteKeyValueEndpoint_ReturnsNotFound_WhenKeyDoesNotExists()
    {
        // Arrange
        const string notPresentKey = "notpresent";

        var client = CreateClient();

        // Act
        var response = await DeleteAsJsonAsync(client, "v1/", new { Key = notPresentKey });

        // Assert
        Assert.That(response, Is.Not.Null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    private Task<HttpResponseMessage> DeleteAsJsonAsync<T>(HttpClient httpClient, string requestUri, T data)
    {
        return httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri) { Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json") }); 
    }

    private async Task CreateKeyValueInPsqlContainerAsync(string key, string value)
    {
        await psqlContainer.ExecScriptAsync($"INSERT INTO \"KeyValues\" VALUES ('{key}', '{value}')");
    }
}