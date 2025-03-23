using GM_Buddy.Contracts;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace GM_Buddy.Data;

public class DbConnector : IDbConnector
{
    private DbSettings _dbSettings;

    public DbConnector(IOptions<DbSettings> dbSettings)
    {
        _dbSettings = dbSettings.Value;
    }

    public IDbConnection CreateConnection()
    {
        var connectionString = $"Server={_dbSettings.Host};Port={_dbSettings.Port};Database={_dbSettings.Database};Username={_dbSettings.Username};Password={_dbSettings.Password};Timeout=300;CommandTimeout=300";
        return new NpgsqlConnection(connectionString);
    }
}
