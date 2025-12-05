using Dapper;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using System.Data;

namespace GM_Buddy.Data;

/// <summary>
/// Repository for managing relationships between entities (NPCs, PCs, Organizations)
/// </summary>
public class RelationshipRepository : IRelationshipRepository
{
    private readonly IDbConnector _dbConnector;

    public RelationshipRepository(IDbConnector dbConnector)
    {
        _dbConnector = dbConnector;
    }

    #region Relationship Type Methods

    public async Task<IEnumerable<RelationshipType>> GetAllRelationshipTypesAsync(CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT 
                rt.relationship_type_id,
                rt.relationship_type_name,
                rt.description,
                rt.is_directional,
                rt.inverse_type_id,
                rt.created_at,
                irt.relationship_type_name as inverse_type_name
            FROM public.relationship_type rt
            LEFT JOIN public.relationship_type irt ON rt.inverse_type_id = irt.relationship_type_id
            ORDER BY rt.relationship_type_name";

        CommandDefinition cmd = new(sql, cancellationToken: ct);
        return await dbConnection.QueryAsync<RelationshipType>(cmd);
    }

    public async Task<RelationshipType?> GetRelationshipTypeByIdAsync(int relationshipTypeId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT 
                rt.relationship_type_id,
                rt.relationship_type_name,
                rt.description,
                rt.is_directional,
                rt.inverse_type_id,
                rt.created_at,
                irt.relationship_type_name as inverse_type_name
            FROM public.relationship_type rt
            LEFT JOIN public.relationship_type irt ON rt.inverse_type_id = irt.relationship_type_id
            WHERE rt.relationship_type_id = @RelationshipTypeId";

        CommandDefinition cmd = new(sql, new { RelationshipTypeId = relationshipTypeId }, cancellationToken: ct);
        return await dbConnection.QueryFirstOrDefaultAsync<RelationshipType>(cmd);
    }

    public async Task<RelationshipType?> GetRelationshipTypeByNameAsync(string name, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT 
                rt.relationship_type_id,
                rt.relationship_type_name,
                rt.description,
                rt.is_directional,
                rt.inverse_type_id,
                rt.created_at,
                irt.relationship_type_name as inverse_type_name
            FROM public.relationship_type rt
            LEFT JOIN public.relationship_type irt ON rt.inverse_type_id = irt.relationship_type_id
            WHERE rt.relationship_type_name = @Name";

        CommandDefinition cmd = new(sql, new { Name = name }, cancellationToken: ct);
        return await dbConnection.QueryFirstOrDefaultAsync<RelationshipType>(cmd);
    }

    #endregion

    #region Entity Relationship Methods

    public async Task<int> CreateRelationshipAsync(EntityRelationship relationship, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            INSERT INTO public.entity_relationship (
                source_entity_type,
                source_entity_id,
                target_entity_type,
                target_entity_id,
                relationship_type_id,
                description,
                strength,
                is_active,
                campaign_id
            )
            VALUES (
                @source_entity_type,
                @source_entity_id,
                @target_entity_type,
                @target_entity_id,
                @relationship_type_id,
                @description,
                @strength,
                @is_active,
                @campaign_id
            )
            RETURNING entity_relationship_id";

        CommandDefinition cmd = new(sql, relationship, cancellationToken: ct);
        return await dbConnection.ExecuteScalarAsync<int>(cmd);
    }

    public async Task<EntityRelationship?> GetRelationshipByIdAsync(int relationshipId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT 
                er.entity_relationship_id,
                er.source_entity_type,
                er.source_entity_id,
                er.target_entity_type,
                er.target_entity_id,
                er.relationship_type_id,
                er.description,
                er.strength,
                er.is_active,
                er.campaign_id,
                er.created_at,
                er.updated_at,
                CASE er.source_entity_type
                    WHEN 'npc' THEN n1.name
                    WHEN 'pc' THEN p1.name
                    WHEN 'organization' THEN o1.name
                END as source_name,
                CASE er.target_entity_type
                    WHEN 'npc' THEN n2.name
                    WHEN 'pc' THEN p2.name
                    WHEN 'organization' THEN o2.name
                END as target_name,
                rt.relationship_type_name,
                rt.is_directional,
                c.name as campaign_name
            FROM public.entity_relationship er
            JOIN public.relationship_type rt ON er.relationship_type_id = rt.relationship_type_id
            LEFT JOIN public.npc n1 ON er.source_entity_type = 'npc' AND er.source_entity_id = n1.npc_id
            LEFT JOIN public.pc p1 ON er.source_entity_type = 'pc' AND er.source_entity_id = p1.pc_id
            LEFT JOIN public.organization o1 ON er.source_entity_type = 'organization' AND er.source_entity_id = o1.organization_id
            LEFT JOIN public.npc n2 ON er.target_entity_type = 'npc' AND er.target_entity_id = n2.npc_id
            LEFT JOIN public.pc p2 ON er.target_entity_type = 'pc' AND er.target_entity_id = p2.pc_id
            LEFT JOIN public.organization o2 ON er.target_entity_type = 'organization' AND er.target_entity_id = o2.organization_id
            LEFT JOIN public.campaign c ON er.campaign_id = c.campaign_id
            WHERE er.entity_relationship_id = @RelationshipId";

        CommandDefinition cmd = new(sql, new { RelationshipId = relationshipId }, cancellationToken: ct);
        return await dbConnection.QueryFirstOrDefaultAsync<EntityRelationship>(cmd);
    }

    public async Task<IEnumerable<EntityRelationship>> GetRelationshipsForEntityAsync(
        string entityType,
        int entityId,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        string sql = BuildRelationshipQuery() + @"
            WHERE (
                (er.source_entity_type = @EntityType AND er.source_entity_id = @EntityId)
                OR (er.target_entity_type = @EntityType AND er.target_entity_id = @EntityId)
            )";

        if (!includeInactive)
        {
            sql += " AND er.is_active = true";
        }

        sql += " ORDER BY er.created_at DESC";

        CommandDefinition cmd = new(sql, new { EntityType = entityType, EntityId = entityId }, cancellationToken: ct);
        return await dbConnection.QueryAsync<EntityRelationship>(cmd);
    }

    public async Task<IEnumerable<EntityRelationship>> GetRelationshipsFromEntityAsync(
        string entityType,
        int entityId,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        string sql = BuildRelationshipQuery() + @"
            WHERE er.source_entity_type = @EntityType AND er.source_entity_id = @EntityId";

        if (!includeInactive)
        {
            sql += " AND er.is_active = true";
        }

        sql += " ORDER BY er.created_at DESC";

        CommandDefinition cmd = new(sql, new { EntityType = entityType, EntityId = entityId }, cancellationToken: ct);
        return await dbConnection.QueryAsync<EntityRelationship>(cmd);
    }

    public async Task<IEnumerable<EntityRelationship>> GetRelationshipsToEntityAsync(
        string entityType,
        int entityId,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        string sql = BuildRelationshipQuery() + @"
            WHERE er.target_entity_type = @EntityType AND er.target_entity_id = @EntityId";

        if (!includeInactive)
        {
            sql += " AND er.is_active = true";
        }

        sql += " ORDER BY er.created_at DESC";

        CommandDefinition cmd = new(sql, new { EntityType = entityType, EntityId = entityId }, cancellationToken: ct);
        return await dbConnection.QueryAsync<EntityRelationship>(cmd);
    }

    public async Task<IEnumerable<EntityRelationship>> GetRelationshipsByTypeAsync(
        string entityType,
        int entityId,
        int relationshipTypeId,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        string sql = BuildRelationshipQuery() + @"
            WHERE (
                (er.source_entity_type = @EntityType AND er.source_entity_id = @EntityId)
                OR (er.target_entity_type = @EntityType AND er.target_entity_id = @EntityId)
            )
            AND er.relationship_type_id = @RelationshipTypeId";

        if (!includeInactive)
        {
            sql += " AND er.is_active = true";
        }

        sql += " ORDER BY er.created_at DESC";

        CommandDefinition cmd = new(sql, new 
        { 
            EntityType = entityType, 
            EntityId = entityId, 
            RelationshipTypeId = relationshipTypeId 
        }, cancellationToken: ct);

        return await dbConnection.QueryAsync<EntityRelationship>(cmd);
    }

    public async Task<IEnumerable<EntityRelationship>> GetRelationshipsByCampaignAsync(
        int campaignId,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        string sql = BuildRelationshipQuery() + " WHERE er.campaign_id = @CampaignId";

        if (!includeInactive)
        {
            sql += " AND er.is_active = true";
        }

        sql += " ORDER BY er.created_at DESC";

        CommandDefinition cmd = new(sql, new { CampaignId = campaignId }, cancellationToken: ct);
        return await dbConnection.QueryAsync<EntityRelationship>(cmd);
    }

    public async Task UpdateRelationshipAsync(EntityRelationship relationship, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            UPDATE public.entity_relationship
            SET description = @description,
                strength = @strength,
                is_active = @is_active,
                campaign_id = @campaign_id,
                updated_at = NOW()
            WHERE entity_relationship_id = @entity_relationship_id";

        CommandDefinition cmd = new(sql, relationship, cancellationToken: ct);
        await dbConnection.ExecuteAsync(cmd);
    }

    public async Task DeleteRelationshipAsync(int relationshipId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = "DELETE FROM public.entity_relationship WHERE entity_relationship_id = @RelationshipId";

        CommandDefinition cmd = new(sql, new { RelationshipId = relationshipId }, cancellationToken: ct);
        await dbConnection.ExecuteAsync(cmd);
    }

    public async Task DeactivateRelationshipAsync(int relationshipId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            UPDATE public.entity_relationship
            SET is_active = false,
                updated_at = NOW()
            WHERE entity_relationship_id = @RelationshipId";

        CommandDefinition cmd = new(sql, new { RelationshipId = relationshipId }, cancellationToken: ct);
        await dbConnection.ExecuteAsync(cmd);
    }

    public async Task ReactivateRelationshipAsync(int relationshipId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            UPDATE public.entity_relationship
            SET is_active = true,
                updated_at = NOW()
            WHERE entity_relationship_id = @RelationshipId";

        CommandDefinition cmd = new(sql, new { RelationshipId = relationshipId }, cancellationToken: ct);
        await dbConnection.ExecuteAsync(cmd);
    }

    public async Task<bool> RelationshipExistsAsync(
        string sourceEntityType,
        int sourceEntityId,
        string targetEntityType,
        int targetEntityId,
        int relationshipTypeId,
        CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT EXISTS(
                SELECT 1 
                FROM public.entity_relationship
                WHERE source_entity_type = @SourceEntityType
                  AND source_entity_id = @SourceEntityId
                  AND target_entity_type = @TargetEntityType
                  AND target_entity_id = @TargetEntityId
                  AND relationship_type_id = @RelationshipTypeId
                  AND is_active = true
            )";

        CommandDefinition cmd = new(sql, new
        {
            SourceEntityType = sourceEntityType,
            SourceEntityId = sourceEntityId,
            TargetEntityType = targetEntityType,
            TargetEntityId = targetEntityId,
            RelationshipTypeId = relationshipTypeId
        }, cancellationToken: ct);

        return await dbConnection.ExecuteScalarAsync<bool>(cmd);
    }

    #endregion

    #region Helper Methods

    private static string BuildRelationshipQuery()
    {
        return @"
            SELECT 
                er.entity_relationship_id,
                er.source_entity_type,
                er.source_entity_id,
                er.target_entity_type,
                er.target_entity_id,
                er.relationship_type_id,
                er.description,
                er.strength,
                er.is_active,
                er.campaign_id,
                er.created_at,
                er.updated_at,
                CASE er.source_entity_type
                    WHEN 'npc' THEN n1.name
                    WHEN 'pc' THEN p1.name
                    WHEN 'organization' THEN o1.name
                END as source_name,
                CASE er.target_entity_type
                    WHEN 'npc' THEN n2.name
                    WHEN 'pc' THEN p2.name
                    WHEN 'organization' THEN o2.name
                END as target_name,
                rt.relationship_type_name,
                rt.is_directional,
                c.name as campaign_name
            FROM public.entity_relationship er
            JOIN public.relationship_type rt ON er.relationship_type_id = rt.relationship_type_id
            LEFT JOIN public.npc n1 ON er.source_entity_type = 'npc' AND er.source_entity_id = n1.npc_id
            LEFT JOIN public.pc p1 ON er.source_entity_type = 'pc' AND er.source_entity_id = p1.pc_id
            LEFT JOIN public.organization o1 ON er.source_entity_type = 'organization' AND er.source_entity_id = o1.organization_id
            LEFT JOIN public.npc n2 ON er.target_entity_type = 'npc' AND er.target_entity_id = n2.npc_id
            LEFT JOIN public.pc p2 ON er.target_entity_type = 'pc' AND er.target_entity_id = p2.pc_id
            LEFT JOIN public.organization o2 ON er.target_entity_type = 'organization' AND er.target_entity_id = o2.organization_id
            LEFT JOIN public.campaign c ON er.campaign_id = c.campaign_id";
    }

    #endregion
}
