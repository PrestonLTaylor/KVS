using Asp.Versioning;
using KVS.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace KVS.Endpoints.v1;

public record struct CreateKeyValueRequest(string NewKey, string Value);

public record struct ReadValueRequest();

public record struct UpdateKeyValueRequest(string Key, string NewValue);

public record struct RemoveKeyRequest(string Key);

public static class KeyValueEndpoints
{
    static public WebApplication MapKeyValueV1Endpoints(this WebApplication app)
    {
        var kvApi = app.NewVersionedApi();
        var v1 = kvApi
            .MapGroup("/v{version:apiVersion}")
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions();

        v1.MapPost("/", HandleCreateKeyValueRequest);

        v1.MapGet("/{key}", HandleReadValueRequest);

        v1.MapPut("/", HandleUpdateKeyValueRequest);

        v1.MapDelete("/", HandleRemoveKeyRequest);

        return app;
    }

    [ProducesResponseType(201)]
    [ProducesResponseType(409)]
    static public IResult HandleCreateKeyValueRequest(ILogger<CreateKeyValueRequest> logger, IKeyValueRepository repo, [FromBody] CreateKeyValueRequest request)
    {
        logger.LogInformation("Creation of key '{Key}' with the value '{Value}' was requested", request.NewKey, request.Value);

        var result = repo.AddKeyValue(request.NewKey, request.Value);

        return result.Match(
            success =>
            {
                logger.LogInformation("Key '{NewKey}' was successfully created with an initial value of '{InitialValue}'", request.NewKey, request.Value);
                return Results.Created($"/{request.NewKey}", request.Value);
            },
            alreadyExists =>
            {
                logger.LogInformation("Creation of key '{Key}' failed as the key already exists", request.NewKey);
                return Results.Conflict($"The key '{request.NewKey}' already exists.");
            }
        );
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    static public IResult HandleReadValueRequest(ILogger<ReadValueRequest> logger, IKeyValueRepository repo, [FromRoute] string key)
    {
        logger.LogInformation("The value of Key '{Key}' was requested", key);

        var result = repo.GetValueByKey(key);

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

    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    static public IResult HandleUpdateKeyValueRequest(ILogger<UpdateKeyValueRequest> logger, IKeyValueRepository repo, [FromBody] UpdateKeyValueRequest request)
    {
        logger.LogInformation("The value of key '{Key}' was requested to be updated with the value '{NewValue}'", request.Key, request.NewValue);

        var result = repo.UpdateKeyValue(request.Key, request.NewValue);

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

    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    static public IResult HandleRemoveKeyRequest(ILogger<RemoveKeyRequest> logger, IKeyValueRepository repo, [FromBody] RemoveKeyRequest request)
    {
        logger.LogInformation("The value of key '{Key}' was requested to be deleted", request.Key);

        var result = repo.RemoveByKey(request.Key);

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
