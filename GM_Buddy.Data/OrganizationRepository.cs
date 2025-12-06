using Dapper;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using System.Data;

namespace GM_Buddy.Data;

/// <summary>
/// Repository for managing Organizations
/// </summary>
public class OrganizationRepository : IOrganizationRepository
{
    private readonly IDbConnector _dbConnector;

    public OrganizationRepository(IDbConnector dbConnector)
    {
        _dbConnector = dbConnector;
    }

    public async Task<IEnumerable<Organization>> GetOrganizationsByAccountIdAsync(int accountId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT 
                organization_id,
                account_id,
                name,
                description,
                created_at,
                updated_at
            FROM public.organization
            WHERE account_id = @AccountId
            ORDER BY created_at DESC";

        CommandDefinition cmd = new(sql, new { AccountId = accountId }, cancellationToken: ct);
        return await dbConnection.QueryAsync<Organization>(cmd);
    }

    public async Task<Organization?> GetOrganizationByIdAsync(int organizationId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT 
                organization_id,
                account_id,
                name,
                description,
                created_at,
                updated_at
            FROM public.organization
            WHERE organization_id = @OrganizationId";

        CommandDefinition cmd = new(sql, new { OrganizationId = organizationId }, cancellationToken: ct);
        return await dbConnection.QueryFirstOrDefaultAsync<Organization>(cmd);
    }

    public async Task<IEnumerable<Organization>> GetOrganizationsByCampaignIdAsync(int campaignId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        
        // Get organizations that have relationships with this campaign
        const string sql = @"
            SELECT DISTINCT
                o.organization_id,
                o.account_id,
                o.name,
                o.description,
                o.created_at,
                o.updated_at
            FROM public.organization o
            INNER JOIN public.entity_relationship er 
                ON er.source_entity_type = 'organization' 
                AND er.source_entity_id = o.organization_id
                AND er.campaign_id = @CampaignId
            WHERE er.is_active = true
            ORDER BY o.created_at DESC";

        CommandDefinition cmd = new(sql, new { CampaignId = campaignId }, cancellationToken: ct);
        return await dbConnection.QueryAsync<Organization>(cmd);
    }

    public async Task<IEnumerable<Organization>> SearchOrganizationsAsync(int accountId, string searchTerm, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT 
                organization_id,
                account_id,
                name,
                description,
                created_at,
                updated_at
            FROM public.organization
            WHERE account_id = @AccountId
              AND (
                  name ILIKE @SearchTerm
                  OR description ILIKE @SearchTerm
              )
            ORDER BY created_at DESC";

        CommandDefinition cmd = new(sql, new 
        { 
            AccountId = accountId, 
            SearchTerm = $"%{searchTerm}%" 
        }, cancellationToken: ct);
        
        return await dbConnection.QueryAsync<Organization>(cmd);
    }

    public async Task<int> CreateOrganizationAsync(Organization organization, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            INSERT INTO public.organization (
                account_id,
                name,
                description
            )
            VALUES (
                @account_id,
                @name,
                @description
            )
            RETURNING organization_id";

        CommandDefinition cmd = new(sql, organization, cancellationToken: ct);
        return await dbConnection.ExecuteScalarAsync<int>(cmd);
    }

    public async Task UpdateOrganizationAsync(Organization organization, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            UPDATE public.organization
            SET name = @name,
                description = @description,
                updated_at = NOW()
            WHERE organization_id = @organization_id";

        CommandDefinition cmd = new(sql, organization, cancellationToken: ct);
        await dbConnection.ExecuteAsync(cmd);
    }

    public async Task DeleteOrganizationAsync(int organizationId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = "DELETE FROM public.organization WHERE organization_id = @OrganizationId";

        CommandDefinition cmd = new(sql, new { OrganizationId = organizationId }, cancellationToken: ct);
        await dbConnection.ExecuteAsync(cmd);
    }

    public async Task<bool> OrganizationExistsAsync(int organizationId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = "SELECT EXISTS(SELECT 1 FROM public.organization WHERE organization_id = @OrganizationId)";

        CommandDefinition cmd = new(sql, new { OrganizationId = organizationId }, cancellationToken: ct);
        return await dbConnection.ExecuteScalarAsync<bool>(cmd);
    }
}
