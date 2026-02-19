using GM_Buddy.Contracts;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace GM_Buddy.Data;

public class DbConnector : IDbConnector
{
    private readonly string _connectionString;

    public DbConnector(IOptions<DbSettings> dbSettings, IConfiguration configuration)
    {
        // Try to get Aspire-provided connection string first
        var aspireConnectionString = configuration.GetConnectionString("gm-buddy-db");

        if (!string.IsNullOrEmpty(aspireConnectionString))
        {
            // Use Aspire's connection string when running in Aspire
            _connectionString = aspireConnectionString;
        }
        else
        {
            // Fall back to manual DbSettings for standalone execution
            var settings = dbSettings.Value;
            _connectionString = $"Server={settings.Host};Port={settings.Port};Database={settings.Database};Username={settings.Username};Password={settings.Password};Timeout=300;CommandTimeout=300";
        }
    }

    public string ConnectionString => _connectionString;

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
