using Asp.Versioning;
using KVS.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace KVS.Endpoints.v1;

public record struct CreateKeyValueRequest(string NewKey, string Value);

public record struct ReadValueRequest();

public record struct UpdateKeyValueRequest(string Key, string NewValue);

public record struct RemoveKeyRequest(string Key);

/// <summary>
/// The class that defines our key value API endpoints
/// </summary>
public static class KeyValueEndpoints
{
    /// <summary>
    /// Maps the our methods to API endpoints for the respective HTTP verbs
    /// </summary>
    static public WebApplication MapKeyValueV1Endpoints(this WebApplication app)
    {
        var kvApi = app.NewVersionedApi();
        var v1 = kvApi
            .MapGroup("/v{version:apiVersion}")
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions();

        v1.MapPost("/", HandleCreateKeyValueRequestAsync);

        v1.MapGet("/{key}", HandleReadValueRequestAsync);

        v1.MapPut("/", HandleUpdateKeyValueRequestAsync);

        v1.MapDelete("/", HandleRemoveKeyRequestAsync);

        return app;
    }

    /// <summary>
    /// Handles a request from a user to create a key value pair inside of our repository.
    /// </summary>
    /// <param name="logger">The logger for <see cref="HandleCreateKeyValueRequestAsync"/>.</param>
    /// <param name="repo">The repository that handles the persistance and caching of key value pairs.</param>
    /// <param name="request">The request from the user that holds the new key and respective value to create.</param>
    /// <returns>
    /// 201 Created if the key value was successfuly created inside our repository.<br/>
    /// 400 Bad Request if either the provided key or value is null.<br/>
    /// 409 Conflict if the key already exists in our repository.<br/>
    /// </returns>
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    static public async Task<IResult> HandleCreateKeyValueRequestAsync(ILogger<CreateKeyValueRequest> logger, IKeyValueRepository repo, 
        [FromBody] CreateKeyValueRequest request)
    {
        if (request.NewKey is null || request.Value is null)
        {
            return Results.BadRequest("The newKey and value field can not be null.");
        }

        logger.LogInformation("Creation of key '{Key}' with the value '{Value}' was requested", request.NewKey, request.Value);

        var result = await repo.AddKeyValueAsync(request.NewKey, request.Value);

        return result.Match(
            success =>
            {
                logger.LogInformation("Key '{NewKey}' was successfully created with an initial value of '{InitialValue}'", request.NewKey, request.Value);
                return Results.Created($"/v1/{request.NewKey}", request.Value);
            },
            alreadyExists =>
            {
                logger.LogInformation("Creation of key '{Key}' failed as the key already exists", request.NewKey);
                return Results.Conflict($"The key '{request.NewKey}' already exists.");
            }
        );
    }

    /// <summary>
    /// Handles a request from a user to read a key value pair from our repository.
    /// </summary>
    /// <param name="logger">The logger for <see cref="HandleReadValueRequestAsync"/>.</param>
    /// <param name="repo">The repository that handles the persistance and caching of key value pairs.</param>
    /// <param name="key">The key that the user is requesting value of.</param>
    /// <returns>
    /// 200 Ok if the respective key value pair was found in the repository.<br/>
    /// 400 Bad Request if provided key is null.<br/>
    /// 404 Not Found if the key was not found in the repository.<br/>
    /// </returns>
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    static public async Task<IResult> HandleReadValueRequestAsync(ILogger<ReadValueRequest> logger, IKeyValueRepository repo,
        [FromRoute] string key)
    {
        if (key is null)
        {
            return Results.BadRequest("The key can not be null.");
        }

        logger.LogInformation("The value of Key '{Key}' was requested", key);

        var result = await repo.GetValueByKeyAsync(key);

        return result.Match(
            success =>
            {
                logger.LogInformation("The key '{Key}' was found with the value of '{Value}'", key, success.Value);
                return Results.Ok(success.Value);
            },
            notFound =>
            {
                logger.LogInformation("The key '{Key}' was not found", key);
                return Results.NotFound($"The key '{key}' was not found.");
            }
        );
    }

    /// <summary>
    /// Handles a request from a user to update a key value pair inside of our repository.
    /// </summary>
    /// <param name="logger">The logger for <see cref="HandleUpdateKeyValueRequestAsync"/>.</param>
    /// <param name="repo">The repository that handles the persistance and caching of key value pairs.</param>
    /// <param name="request">The request from the user that holds an existing key to update and the respective value.</param>
    /// <returns>
    /// 204 No Content if the respective key value pair was found and updated in the repository.<br/>
    /// 400 Bad Request if either the provided key or value is null.<br/>
    /// 404 Not Found if the key was not found in the repository.<br/>
    /// </returns>
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    static public async Task<IResult> HandleUpdateKeyValueRequestAsync(ILogger<UpdateKeyValueRequest> logger, IKeyValueRepository repo,
        [FromBody] UpdateKeyValueRequest request)
    {
        if (request.Key is null || request.NewValue is null)
        {
            return Results.BadRequest("The key and newValue can not be null.");
        }

        logger.LogInformation("The value of key '{Key}' was requested to be updated with the value '{NewValue}'", request.Key, request.NewValue);

        var result = await repo.UpdateKeyValueAsync(request.Key, request.NewValue);

        return result.Match(
            success =>
            {
                logger.LogInformation("The key '{Key}' was updated to the value '{Value}'", request.Key, request.NewValue);
                return Results.NoContent();
            },
            notFound =>
            {
                logger.LogInformation("The key '{Key}' was not found", request.Key);
                return Results.NotFound($"The key '{request.Key}' was not found.");
            }
        );
    }

    /// <summary>
    /// Handles a request from a user to delete a key value pair from our repository.
    /// </summary>
    /// <param name="logger">The logger for <see cref="HandleRemoveKeyRequestAsync"/>.</param>
    /// <param name="repo">The repository that handles the persistance and caching of key value pairs.</param>
    /// <param name="request">The request from the user that holds an existing key.</param>
    /// <returns>
    /// 204 No Content if the respective key value pair was found and deleted in the repository.<br/>
    /// 400 Bad Request if the provided key is null.<br/>
    /// 404 Not Found if the key was not found in the repository.<br/>
    /// </returns>
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    static public async Task<IResult> HandleRemoveKeyRequestAsync(ILogger<RemoveKeyRequest> logger, IKeyValueRepository repo,
        [FromBody] RemoveKeyRequest request)
    {
        if (request.Key is null)
        {
            return Results.BadRequest("The key can not be null.");
        }

        logger.LogInformation("The value of key '{Key}' was requested to be deleted", request.Key);

        var result = await repo.RemoveByKeyAsync(request.Key);

        return result.Match(
            success =>
            {
                logger.LogInformation("The key '{Key}' was deleted", request.Key);
                return Results.NoContent();
            },
            notFound =>
            {
                logger.LogInformation("The key '{Key}' was not found", request.Key);
                return Results.NotFound($"The key '{request.Key}' was not found.");
            }
        );
    }
}
