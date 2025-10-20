using Dapper;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using System.Data;
using System.Threading;

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
                   n.user_id,
                   n.game_system_id,
                   n.lineage_id,
                   n.occupation_id,
                   n.name,
                   n.stats,
                   n.description,
                   n.gender
            FROM npc AS n
            WHERE n.user_id = @AccountId
            ORDER BY n.npc_id";
        var cmd = new CommandDefinition(sql, new { AccountId = account_id }, cancellationToken: ct);
        return await dbConnection.QueryAsync<Npc>(cmd);
    }

    public async Task<Npc?> GetNpcById(int npc_id, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT n.npc_id,
                   n.user_id,
                   n.game_system_id,
                   n.lineage_id,
                   n.occupation_id,
                   n.name,
                   n.stats,
                   n.description,
                   n.gender
            FROM npc AS n
            WHERE n.npc_id = @NpcId";
        var cmd = new CommandDefinition(sql, new { NpcId = npc_id }, cancellationToken: ct);
        return await dbConnection.QueryFirstOrDefaultAsync<Npc>(cmd);
    }
}