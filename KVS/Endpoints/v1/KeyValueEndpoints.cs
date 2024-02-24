using KVS.Repositories;

namespace KVS.Endpoints.v1;

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
}
