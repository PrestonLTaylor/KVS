using Microsoft.EntityFrameworkCore;

namespace KVS.Data;

public static class DevelopmentDatabaseCreation
{
    static public async Task<WebApplication> EnsureDatabaseIsUpToDateAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        await databaseContext.Database.MigrateAsync();

        return app;
    }
}