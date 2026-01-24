using Dapper;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using System.Data;

namespace GM_Buddy.Data;

public class GameSystemRepository : IGameSystemRepository
{
    private readonly IDbConnector _dbConnector;

    public GameSystemRepository(IDbConnector dbConnector)
    {
        _dbConnector = dbConnector;
    }

    public async Task<IEnumerable<Game_System>> GetAllAsync(CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT game_system_id, game_system_name
            FROM game_system
            ORDER BY game_system_name";
        
        var cmd = new CommandDefinition(sql, cancellationToken: ct);
        return await dbConnection.QueryAsync<Game_System>(cmd);
    }

    public async Task<Game_System?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT game_system_id, game_system_name
            FROM game_system
            WHERE game_system_id = @Id";
        
        var cmd = new CommandDefinition(sql, new { Id = id }, cancellationToken: ct);
        return await dbConnection.QueryFirstOrDefaultAsync<Game_System>(cmd);
    }

    public async Task<Game_System?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT game_system_id, game_system_name
            FROM game_system
            WHERE game_system_name = @Name";
        
        var cmd = new CommandDefinition(sql, new { Name = name }, cancellationToken: ct);
        return await dbConnection.QueryFirstOrDefaultAsync<Game_System>(cmd);
    }
}
