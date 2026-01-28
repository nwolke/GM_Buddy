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
                   n.campaign_id,
                   n.name,
                   n.description,
                   n.stats,
                   c.game_system_id,
                   gs.game_system_name
            FROM npc AS n
            JOIN campaign AS c ON n.campaign_id = c.campaign_id
            JOIN game_system AS gs ON c.game_system_id = gs.game_system_id
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
                   n.campaign_id,
                   n.name,
                   n.description,
                   n.stats,
                   c.game_system_id,
                   gs.game_system_name
            FROM npc AS n
            JOIN campaign AS c ON n.campaign_id = c.campaign_id
            JOIN game_system AS gs ON c.game_system_id = gs.game_system_id
            WHERE n.npc_id = @NpcId";
        var cmd = new CommandDefinition(sql, new { NpcId = npc_id }, cancellationToken: ct);
        return await dbConnection.QueryFirstOrDefaultAsync<Npc>(cmd);
    }

    public async Task<int> CreateNpcAsync(Npc npc, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            INSERT INTO npc (account_id, campaign_id, name, description, stats)
            VALUES (@account_id, @campaign_id, @name, @description, @stats::jsonb)
            RETURNING npc_id";
        var cmd = new CommandDefinition(sql, npc, cancellationToken: ct);
        return await dbConnection.ExecuteScalarAsync<int>(cmd);
    }

    public async Task<bool> UpdateNpcAsync(Npc npc, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            UPDATE npc 
            SET name = @name,
                description = @description,
                stats = @stats::jsonb,
                campaign_id = @campaign_id
            WHERE npc_id = @npc_id AND account_id = @account_id";
        var cmd = new CommandDefinition(sql, npc, cancellationToken: ct);
        int rowsAffected = await dbConnection.ExecuteAsync(cmd);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteNpcAsync(int npcId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = "DELETE FROM npc WHERE npc_id = @NpcId";
        var cmd = new CommandDefinition(sql, new { NpcId = npcId }, cancellationToken: ct);
        int rowsAffected = await dbConnection.ExecuteAsync(cmd);
        return rowsAffected > 0;
    }
}