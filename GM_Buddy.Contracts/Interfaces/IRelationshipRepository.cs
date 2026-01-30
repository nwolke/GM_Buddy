using GM_Buddy.Contracts.DbEntities;

namespace GM_Buddy.Contracts.Interfaces;

/// <summary>
/// Repository interface for managing relationships between entities (NPCs, PCs, Organizations)
/// </summary>
public interface IRelationshipRepository
{
    // Relationship Type Methods
    
    /// <summary>
    /// Get all relationship types
    /// </summary>
    Task<IEnumerable<RelationshipType>> GetAllRelationshipTypesAsync(CancellationToken ct = default);

    /// <summary>
    /// Get a specific relationship type by ID
    /// </summary>
    Task<RelationshipType?> GetRelationshipTypeByIdAsync(int relationshipTypeId, CancellationToken ct = default);

    /// <summary>
    /// Get a specific relationship type by name
    /// </summary>
    Task<RelationshipType?> GetRelationshipTypeByNameAsync(string name, CancellationToken ct = default);

    // Entity Relationship Methods

    /// <summary>
    /// Create a new relationship between two entities
    /// </summary>
    Task<int> CreateRelationshipAsync(EntityRelationship relationship, CancellationToken ct = default);

    /// <summary>
    /// Get a specific relationship by ID
    /// </summary>
    Task<EntityRelationship?> GetRelationshipByIdAsync(int relationshipId, CancellationToken ct = default);
    Task<IEnumerable<EntityRelationship>> GetAllRelationshipsOfAccountAsync(
        int accountId,
        bool includeInactive = false,
        CancellationToken ct = default);

    /// <summary>
    /// Get all relationships for a specific entity (as source or target)
    /// </summary>
    Task<IEnumerable<EntityRelationship>> GetRelationshipsForEntityAsync(
        string entityType, 
        int entityId, 
        bool includeInactive = false, 
        CancellationToken ct = default);

    /// <summary>
    /// Get all relationships where the entity is the source
    /// </summary>
    Task<IEnumerable<EntityRelationship>> GetRelationshipsFromEntityAsync(
        string entityType, 
        int entityId, 
        bool includeInactive = false, 
        CancellationToken ct = default);

    /// <summary>
    /// Get all relationships where the entity is the target
    /// </summary>
    Task<IEnumerable<EntityRelationship>> GetRelationshipsToEntityAsync(
        string entityType, 
        int entityId, 
        bool includeInactive = false, 
        CancellationToken ct = default);

    /// <summary>
    /// Get all relationships of a specific type for an entity
    /// </summary>
    Task<IEnumerable<EntityRelationship>> GetRelationshipsByTypeAsync(
        string entityType, 
        int entityId, 
        int relationshipTypeId, 
        bool includeInactive = false, 
        CancellationToken ct = default);

    /// <summary>
    /// Get all relationships within a campaign
    /// </summary>
    Task<IEnumerable<EntityRelationship>> GetRelationshipsByCampaignAsync(
        int campaignId, 
        bool includeInactive = false, 
        CancellationToken ct = default);

    /// <summary>
    /// Update a relationship
    /// </summary>
    Task UpdateRelationshipAsync(EntityRelationship relationship, CancellationToken ct = default);

    /// <summary>
    /// Delete a relationship (hard delete)
    /// </summary>
    Task DeleteRelationshipAsync(int relationshipId, CancellationToken ct = default);

    /// <summary>
    /// Deactivate a relationship (soft delete)
    /// </summary>
    Task DeactivateRelationshipAsync(int relationshipId, CancellationToken ct = default);

    /// <summary>
    /// Reactivate a previously deactivated relationship
    /// </summary>
    Task ReactivateRelationshipAsync(int relationshipId, CancellationToken ct = default);

    /// <summary>
    /// Check if a relationship exists between two entities
    /// </summary>
    Task<bool> RelationshipExistsAsync(
        string sourceEntityType, 
        int sourceEntityId, 
        string targetEntityType, 
        int targetEntityId, 
        int relationshipTypeId, 
        CancellationToken ct = default);
}
