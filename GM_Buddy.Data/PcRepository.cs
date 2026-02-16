using Dapper;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using System.Data;

namespace GM_Buddy.Data;

/// <summary>
/// Repository for managing Player Characters (PCs)
/// </summary>
public class PcRepository : IPcRepository
{
    private readonly IDbConnector _dbConnector;

    public PcRepository(IDbConnector dbConnector)
    {
        _dbConnector = dbConnector;
    }

    public async Task<IEnumerable<Pc>> GetPcsByAccountIdAsync(int accountId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT 
                p.pc_id,
                p.account_id,
                p.name,
                p.description,
                p.created_at,
                p.updated_at
            FROM public.pc p
            WHERE p.account_id = @AccountId
            ORDER BY p.created_at DESC";

        CommandDefinition cmd = new(sql, new { AccountId = accountId }, cancellationToken: ct);
        return await dbConnection.QueryAsync<Pc>(cmd);
    }

    public async Task<Pc?> GetPcByIdAsync(int pcId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT 
                p.pc_id,
                p.account_id,
                p.name,
                p.description,
                p.created_at,
                p.updated_at
            FROM public.pc p
            WHERE p.pc_id = @PcId";

        CommandDefinition cmd = new(sql, new { PcId = pcId }, cancellationToken: ct);
        return await dbConnection.QueryFirstOrDefaultAsync<Pc>(cmd);
    }

    public async Task<IEnumerable<Pc>> GetPcsByCampaignIdAsync(int campaignId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        
        // Get PCs that have relationships with this campaign
        // This assumes PCs are linked to campaigns via entity_relationship table
        const string sql = @"
            SELECT DISTINCT
                p.pc_id,
                p.account_id,
                p.name,
                p.description,
                p.created_at,
                p.updated_at
            FROM public.pc p
            INNER JOIN public.entity_relationship er 
                ON er.source_entity_type = 'pc' 
                AND er.source_entity_id = p.pc_id
                AND er.campaign_id = @CampaignId
            WHERE er.is_active = true
            ORDER BY p.created_at DESC";

        CommandDefinition cmd = new(sql, new { CampaignId = campaignId }, cancellationToken: ct);
        return await dbConnection.QueryAsync<Pc>(cmd);
    }

    public async Task<int> CreatePcAsync(Pc pc, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            INSERT INTO public.pc (
                account_id,
                name,
                description
            )
            VALUES (
                @account_id,
                @name,
                @description
            )
            RETURNING pc_id";

        CommandDefinition cmd = new(sql, pc, cancellationToken: ct);
        return await dbConnection.ExecuteScalarAsync<int>(cmd);
    }

    public async Task UpdatePcAsync(Pc pc, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            UPDATE public.pc
            SET name = @name,
                description = @description,
                updated_at = NOW()
            WHERE pc_id = @pc_id";

        CommandDefinition cmd = new(sql, pc, cancellationToken: ct);
        await dbConnection.ExecuteAsync(cmd);
    }

    public async Task DeletePcAsync(int pcId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = "DELETE FROM public.pc WHERE pc_id = @PcId";

        CommandDefinition cmd = new(sql, new { PcId = pcId }, cancellationToken: ct);
        await dbConnection.ExecuteAsync(cmd);
    }

    public async Task<bool> PcExistsAsync(int pcId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = "SELECT EXISTS(SELECT 1 FROM public.pc WHERE pc_id = @PcId)";

        CommandDefinition cmd = new(sql, new { PcId = pcId }, cancellationToken: ct);
        return await dbConnection.ExecuteScalarAsync<bool>(cmd);
    }
}
