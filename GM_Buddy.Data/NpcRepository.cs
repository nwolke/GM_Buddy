using Dapper;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using System.Data;

namespace GM_Buddy.Data;

public class NpcRepository : INpcRepository
{
    private readonly IDbConnector _dbConnector;

    public NpcRepository(IDbConnector dbConnector)
    {
        _dbConnector = dbConnector;
    }

    public async Task<IEnumerable<Npc>> GetNpcsByAccountId(int account_id, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT n.npc_id,
                   n.account_id,
                   n.game_system_id,
                   n.name,
                   n.description,
                   n.stats,
                   gs.game_system_name
            FROM npc AS n
            JOIN game_system AS gs ON n.game_system_id = gs.game_system_id
            WHERE n.account_id = @AccountId
            ORDER BY n.npc_id";
        var cmd = new CommandDefinition(sql, new { AccountId = account_id }, cancellationToken: ct);
        return await dbConnection.QueryAsync<Npc>(cmd);
    }

    public async Task<Npc?> GetNpcById(int npc_id, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT n.npc_id,
                   n.account_id,
                   n.game_system_id,
                   n.name,
                   n.description,
                   n.stats,
                   gs.game_system_name
            FROM npc AS n
            JOIN game_system AS gs ON n.game_system_id = gs.game_system_id
            WHERE n.npc_id = @NpcId";
        var cmd = new CommandDefinition(sql, new { NpcId = npc_id }, cancellationToken: ct);
        return await dbConnection.QueryFirstOrDefaultAsync<Npc>(cmd);
    }
}