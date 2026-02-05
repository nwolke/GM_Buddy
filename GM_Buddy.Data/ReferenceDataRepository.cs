using Dapper;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using System.Data;

namespace GM_Buddy.Data;

public class ReferenceDataRepository : IReferenceDataRepository
{
    private readonly IDbConnector _dbConnector;

    public ReferenceDataRepository(IDbConnector dbConnector)
    {
        _dbConnector = dbConnector;
    }

    // Lineage methods
    public async Task<IEnumerable<ReferenceLineage>> GetLineagesAsync(int gameSystemId, int? accountId = null, int? campaignId = null, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT lineage_id, game_system_id, account_id, campaign_id, name, description, is_active, created_at, updated_at
            FROM reference_lineage
            WHERE game_system_id = @GameSystemId 
              AND is_active = true 
              AND (
                  (account_id IS NULL AND campaign_id IS NULL) 
                  OR (@AccountId IS NOT NULL AND @CampaignId IS NOT NULL 
                      AND account_id = @AccountId AND campaign_id = @CampaignId)
              )
            ORDER BY account_id NULLS FIRST, campaign_id NULLS FIRST, name";
        
        var cmd = new CommandDefinition(sql, new { GameSystemId = gameSystemId, AccountId = accountId, CampaignId = campaignId }, cancellationToken: ct);
        return await dbConnection.QueryAsync<ReferenceLineage>(cmd);
    }

    public async Task<ReferenceLineage?> GetLineageByIdAsync(int lineageId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT lineage_id, game_system_id, account_id, campaign_id, name, description, is_active, created_at, updated_at
            FROM reference_lineage
            WHERE lineage_id = @LineageId";
        
        var cmd = new CommandDefinition(sql, new { LineageId = lineageId }, cancellationToken: ct);
        return await dbConnection.QueryFirstOrDefaultAsync<ReferenceLineage>(cmd);
    }

    public async Task<int> CreateLineageAsync(ReferenceLineage lineage, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            INSERT INTO reference_lineage (game_system_id, account_id, campaign_id, name, description, is_active)
            VALUES (@game_system_id, @account_id, @campaign_id, @name, @description, @is_active)
            RETURNING lineage_id";
        
        var cmd = new CommandDefinition(sql, lineage, cancellationToken: ct);
        return await dbConnection.ExecuteScalarAsync<int>(cmd);
    }

    public async Task<bool> UpdateLineageAsync(ReferenceLineage lineage, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            UPDATE reference_lineage
            SET name = @name,
                description = @description,
                is_active = @is_active,
                updated_at = NOW()
            WHERE lineage_id = @lineage_id 
              AND account_id = @account_id 
              AND campaign_id = @campaign_id";
        
        var cmd = new CommandDefinition(sql, lineage, cancellationToken: ct);
        int rowsAffected = await dbConnection.ExecuteAsync(cmd);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteLineageAsync(int lineageId, int accountId, int? campaignId = null, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            DELETE FROM reference_lineage 
            WHERE lineage_id = @LineageId 
              AND account_id = @AccountId 
              AND campaign_id = @CampaignId";
        
        var cmd = new CommandDefinition(sql, new { LineageId = lineageId, AccountId = accountId, CampaignId = campaignId }, cancellationToken: ct);
        int rowsAffected = await dbConnection.ExecuteAsync(cmd);
        return rowsAffected > 0;
    }

    // Occupation methods
    public async Task<IEnumerable<ReferenceOccupation>> GetOccupationsAsync(int gameSystemId, int? accountId = null, int? campaignId = null, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT occupation_id, game_system_id, account_id, campaign_id, name, description, is_active, created_at, updated_at
            FROM reference_occupation
            WHERE game_system_id = @GameSystemId 
              AND is_active = true 
              AND (
                  (account_id IS NULL AND campaign_id IS NULL) 
                  OR (@AccountId IS NOT NULL AND @CampaignId IS NOT NULL 
                      AND account_id = @AccountId AND campaign_id = @CampaignId)
              )
            ORDER BY account_id NULLS FIRST, campaign_id NULLS FIRST, name";
        
        var cmd = new CommandDefinition(sql, new { GameSystemId = gameSystemId, AccountId = accountId, CampaignId = campaignId }, cancellationToken: ct);
        return await dbConnection.QueryAsync<ReferenceOccupation>(cmd);
    }

    public async Task<ReferenceOccupation?> GetOccupationByIdAsync(int occupationId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT occupation_id, game_system_id, account_id, campaign_id, name, description, is_active, created_at, updated_at
            FROM reference_occupation
            WHERE occupation_id = @OccupationId";
        
        var cmd = new CommandDefinition(sql, new { OccupationId = occupationId }, cancellationToken: ct);
        return await dbConnection.QueryFirstOrDefaultAsync<ReferenceOccupation>(cmd);
    }

    public async Task<int> CreateOccupationAsync(ReferenceOccupation occupation, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            INSERT INTO reference_occupation (game_system_id, account_id, campaign_id, name, description, is_active)
            VALUES (@game_system_id, @account_id, @campaign_id, @name, @description, @is_active)
            RETURNING occupation_id";
        
        var cmd = new CommandDefinition(sql, occupation, cancellationToken: ct);
        return await dbConnection.ExecuteScalarAsync<int>(cmd);
    }

    public async Task<bool> UpdateOccupationAsync(ReferenceOccupation occupation, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            UPDATE reference_occupation
            SET name = @name,
                description = @description,
                is_active = @is_active,
                updated_at = NOW()
            WHERE occupation_id = @occupation_id 
              AND account_id = @account_id 
              AND campaign_id = @campaign_id";
        
        var cmd = new CommandDefinition(sql, occupation, cancellationToken: ct);
        int rowsAffected = await dbConnection.ExecuteAsync(cmd);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteOccupationAsync(int occupationId, int accountId, int? campaignId = null, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            DELETE FROM reference_occupation 
            WHERE occupation_id = @OccupationId 
              AND account_id = @AccountId 
              AND campaign_id = @CampaignId";
        
        var cmd = new CommandDefinition(sql, new { OccupationId = occupationId, AccountId = accountId, CampaignId = campaignId }, cancellationToken: ct);
        int rowsAffected = await dbConnection.ExecuteAsync(cmd);
        return rowsAffected > 0;
    }
}
