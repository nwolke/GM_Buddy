using GM_Buddy.Contracts;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace GM_Buddy.Data;

public class DbConnector : IDbConnector
{
    private readonly DbSettings _dbSettings;
    private readonly string _connectionString;

    public DbConnector(IOptions<DbSettings> dbSettings)
    {
        _dbSettings = dbSettings.Value;
        _connectionString = $"Server={_dbSettings.Host};Port={_dbSettings.Port};Database={_dbSettings.Database};Username={_dbSettings.Username};Password={_dbSettings.Password};Timeout=300;CommandTimeout=300";
    }

    public string ConnectionString => _connectionString;

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
