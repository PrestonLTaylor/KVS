using KVS.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace KVS.Endpoints.v1;

public record struct CreateKeyValueRequest(string NewKey, string Value);

public static class KeyValueEndpoints
{
    static public WebApplication MapKeyValueV1Endpoints(this WebApplication app)
    {
        var kvApi = app.NewVersionedApi();
        var v1 = kvApi.MapGroup("/v1").HasApiVersion(1.0);

        v1.MapPost("/", (IKeyValueRepository repo) =>
        {
            return Results.Created();
        });

        v1.MapGet("/", (IKeyValueRepository repo) =>
        {
            return Results.Ok();
        });

        v1.MapPut("/", (IKeyValueRepository repo) =>
        {
            return Results.NoContent();
        });

        v1.MapDelete("/", (IKeyValueRepository repo) =>
        {
            return Results.NoContent();
        });

        return app;
    }

    static public IResult HandleCreateKeyValueRequest(ILogger<CreateKeyValueRequest> logger, IKeyValueRepository repo, [FromBody] CreateKeyValueRequest request)
    {
        logger.LogInformation("Creation of key '{Key}' with the value '{Value}' was requested", request.NewKey, request.Value);

        var result = repo.AddKeyValue(request.NewKey, request.Value);

        return result.Match(
            success =>
            {
                logger.LogInformation("Key '{NewKey}' was successfully created with an initial value of '{InitialValue}'", request.NewKey, request.Value);
                return Results.Created("/", request.Value);
            },
            alreadyExists =>
            {
                logger.LogInformation("Creation of key '{Key}' failed as the key already exists", request.NewKey);
                return Results.Conflict($"The key '{request.NewKey}' already exists.");
            }
        );
    }
}
