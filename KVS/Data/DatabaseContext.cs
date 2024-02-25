using Microsoft.EntityFrameworkCore;

namespace KVS.Data;

public sealed class DatabaseContext(ILogger<DatabaseContext> _logger, IConfiguration _configuration) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var connectionString = GetPostgresConnectionString();
        UseNpgsql(options, connectionString);
    }

    private string GetPostgresConnectionString()
    {
        var connectionString = _configuration.GetValue<string>(CONNECTION_STRING_VARIABLE);
        if (connectionString is null)
        {
            _logger.LogCritical("Unable to find connection string '{CONNECTION_STRING_VARIABLE}' for postgres database.", CONNECTION_STRING_VARIABLE);
            throw new InvalidOperationException($"Unable to find connection string '{CONNECTION_STRING_VARIABLE}' for postgres database.");
        }

        return connectionString;
    }

    private void UseNpgsql(DbContextOptionsBuilder options, string connectionString)
    {
        try
        {
            options.UseNpgsql(connectionString);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unable to connect to postgres database.");
            throw;
        }
    }

    const string CONNECTION_STRING_VARIABLE = "POSTGRESQLCONNSTR_DefaultConnection";
}
