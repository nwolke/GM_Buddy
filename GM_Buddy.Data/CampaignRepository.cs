using Dapper;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using System.Data;

namespace GM_Buddy.Data;

public class CampaignRepository : ICampaignRepository
{
    private readonly IDbConnector _dbConnector;
    public CampaignRepository(IDbConnector dbConnector)
    {
        _dbConnector = dbConnector;
    }

    public async Task<int> CreateAsync(Campaign campaign, CancellationToken ct= default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            INSERT INTO campaign (account_id, game_system_id, name, description)
            VALUES (@account_id, @game_system_id, @name, @description)
            RETURNING campaign_id";
        var cmd = new CommandDefinition(sql, campaign, cancellationToken: ct);
        return await dbConnection.ExecuteScalarAsync<int>(cmd);
    }

    public async Task<bool> DeleteAsync(int campaignId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"DELETE FROM campaign WHERE campaign_id = @CampaignId";
        var cmd = new CommandDefinition(sql, new { CampaignId = campaignId }, cancellationToken: ct);
        int rowsAffected = await dbConnection.ExecuteAsync(cmd);
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<Campaign>> GetByAccountIdAsync(int accountId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT c.campaign_id,
                   c.account_id, 
                   c.game_system_id, 
                   c.name, 
                   c.description,
                   c.created_at,
                   c.updated_at
            FROM campaign c
            LEFT JOIN game_system gs ON c.game_system_id = gs.game_system_id
            WHERE c.account_id = @AccountId
            ORDER BY c.updated_at DESC";
        var cmd = new CommandDefinition(sql, new { AccountId = accountId }, cancellationToken: ct);
        return await dbConnection.QueryAsync<Campaign>(cmd);
    }

    public async Task<Campaign?> GetByIdAsync(int campaignId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT c.campaign_id,
                   c.account_id, 
                   c.game_system_id, 
                   c.name, 
                   c.description,
                   c.created_at,
                   c.updated_at,
                   gs.game_system_name
            FROM campaign c
            LEFT JOIN game_system gs ON c.game_system_id = gs.game_system_id
            WHERE c.campaign_id = @CampaignId";
        var cmd = new CommandDefinition(sql, new { CampaignId = campaignId }, cancellationToken: ct);
        return await dbConnection.QueryFirstOrDefaultAsync<Campaign>(cmd);
    }

    public async Task<bool> UpdateAsync(Campaign campaign, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            UPDATE campaign 
            SET name = @name,
                description = @description
            WHERE campaign_id = @campaign_id AND account_id = @account_id";
        var cmd = new CommandDefinition(sql, campaign, cancellationToken: ct);
        var rowsAffected = await dbConnection.ExecuteAsync(cmd);
        return rowsAffected > 0;
    }
}
