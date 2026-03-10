using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;

namespace GM_Buddy.Business.UnitTests;

/// <summary>
/// Fake in-memory implementation of IRelationshipRepository for testing
/// </summary>
internal class FakeRelationshipRepository : IRelationshipRepository
{
    private readonly List<RelationshipType> _relationshipTypes;
    private readonly List<EntityRelationship> _relationships;
    private int _nextRelationshipId = 1;

    public FakeRelationshipRepository(
        IEnumerable<RelationshipType>? relationshipTypes = null,
        IEnumerable<EntityRelationship>? relationships = null)
    {
        _relationshipTypes = relationshipTypes?.ToList() ?? CreateDefaultRelationshipTypes();
        _relationships = relationships?.ToList() ?? new List<EntityRelationship>();

        if (_relationships.Any())
        {
            _nextRelationshipId = _relationships.Max(r => r.entity_relationship_id) + 1;
        }
    }

    private static List<RelationshipType> CreateDefaultRelationshipTypes()
    {
        return new List<RelationshipType>
        {
            new() { relationship_type_id = 1, relationship_type_name = "Acquaintance", is_directional = false },
            new() { relationship_type_id = 2, relationship_type_name = "Ally", is_directional = false },
            new() { relationship_type_id = 3, relationship_type_name = "Child", is_directional = true, inverse_type_id = 16 },
            new() { relationship_type_id = 4, relationship_type_name = "Contact", is_directional = false },
            new() { relationship_type_id = 5, relationship_type_name = "Employee", is_directional = true, inverse_type_id = 6 },
            new() { relationship_type_id = 6, relationship_type_name = "Employer", is_directional = true, inverse_type_id = 5 },
            new() { relationship_type_id = 7, relationship_type_name = "Enemy", is_directional = false },
            new() { relationship_type_id = 8, relationship_type_name = "Family", is_directional = false },
            new() { relationship_type_id = 9, relationship_type_name = "Follower", is_directional = true, inverse_type_id = 12 },
            new() { relationship_type_id = 10, relationship_type_name = "Friend", is_directional = false },
            new() { relationship_type_id = 11, relationship_type_name = "Informant", is_directional = true },
            new() { relationship_type_id = 12, relationship_type_name = "Leader", is_directional = true, inverse_type_id = 9 },
            new() { relationship_type_id = 13, relationship_type_name = "Lover", is_directional = false },
            new() { relationship_type_id = 14, relationship_type_name = "Member", is_directional = true },
            new() { relationship_type_id = 15, relationship_type_name = "Mentor", is_directional = true, inverse_type_id = 23 },
            new() { relationship_type_id = 16, relationship_type_name = "Parent", is_directional = true, inverse_type_id = 3 },
            new() { relationship_type_id = 17, relationship_type_name = "Patron", is_directional = true, inverse_type_id = 18 },
            new() { relationship_type_id = 18, relationship_type_name = "Protege", is_directional = true, inverse_type_id = 17 },
            new() { relationship_type_id = 19, relationship_type_name = "Rival", is_directional = false },
            new() { relationship_type_id = 20, relationship_type_name = "Sibling", is_directional = false },
            new() { relationship_type_id = 21, relationship_type_name = "Spouse", is_directional = false },
            new() { relationship_type_id = 22, relationship_type_name = "Stranger", is_directional = false },
            new() { relationship_type_id = 23, relationship_type_name = "Student", is_directional = true, inverse_type_id = 15 },
            new() { relationship_type_id = 24, relationship_type_name = "Vassal", is_directional = true }
        };
    }

    #region Relationship Type Methods

    public Task<IEnumerable<RelationshipType>> GetAllRelationshipTypesAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_relationshipTypes.AsEnumerable());
    }

    public Task<RelationshipType?> GetRelationshipTypeByIdAsync(int relationshipTypeId, CancellationToken ct = default)
    {
        var type = _relationshipTypes.FirstOrDefault(rt => rt.relationship_type_id == relationshipTypeId);
        return Task.FromResult(type);
    }

    public Task<RelationshipType?> GetRelationshipTypeByNameAsync(string name, CancellationToken ct = default)
    {
        var type = _relationshipTypes.FirstOrDefault(rt =>
            rt.relationship_type_name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(type);
    }

    #endregion

    #region Entity Relationship Methods

    public Task<int> CreateRelationshipAsync(EntityRelationship relationship, CancellationToken ct = default)
    {
        relationship.entity_relationship_id = _nextRelationshipId++;
        relationship.created_at = DateTime.UtcNow;
        relationship.updated_at = DateTime.UtcNow;
        _relationships.Add(relationship);
        return Task.FromResult(relationship.entity_relationship_id);
    }

    public Task<EntityRelationship?> GetRelationshipByIdAsync(int relationshipId, int accountId, CancellationToken ct = default)
    {
        // Fake: accepts accountId but doesn't filter — auth tested at controller level
        var relationship = _relationships.FirstOrDefault(r => r.entity_relationship_id == relationshipId);
        return Task.FromResult(relationship);
    }

    public Task<IEnumerable<EntityRelationship>> GetRelationshipsForEntityAsync(
        string entityType,
        int entityId,
        int accountId,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        // Fake: accepts accountId but doesn't filter — auth tested at controller level
        var result = _relationships.Where(r =>
            ((r.source_entity_type == entityType && r.source_entity_id == entityId) ||
             (r.target_entity_type == entityType && r.target_entity_id == entityId)) &&
            (includeInactive || r.is_active));

        return Task.FromResult(result.AsEnumerable());
    }

    public Task<IEnumerable<EntityRelationship>> GetRelationshipsFromEntityAsync(
        string entityType,
        int entityId,
        int accountId,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        // Fake: accepts accountId but doesn't filter — auth tested at controller level
        var result = _relationships.Where(r =>
            r.source_entity_type == entityType &&
            r.source_entity_id == entityId &&
            (includeInactive || r.is_active));

        return Task.FromResult(result.AsEnumerable());
    }

    public Task<IEnumerable<EntityRelationship>> GetRelationshipsToEntityAsync(
        string entityType,
        int entityId,
        int accountId,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        // Fake: accepts accountId but doesn't filter — auth tested at controller level
        var result = _relationships.Where(r =>
            r.target_entity_type == entityType &&
            r.target_entity_id == entityId &&
            (includeInactive || r.is_active));

        return Task.FromResult(result.AsEnumerable());
    }

    public Task<IEnumerable<EntityRelationship>> GetRelationshipsByTypeAsync(
        string entityType,
        int entityId,
        int relationshipTypeId,
        int accountId,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        // Fake: accepts accountId but doesn't filter — auth tested at controller level
        var result = _relationships.Where(r =>
            ((r.source_entity_type == entityType && r.source_entity_id == entityId) ||
             (r.target_entity_type == entityType && r.target_entity_id == entityId)) &&
            r.relationship_type_id == relationshipTypeId &&
            (includeInactive || r.is_active));

        return Task.FromResult(result.AsEnumerable());
    }

    public Task<IEnumerable<EntityRelationship>> GetRelationshipsByCampaignAsync(
        int campaignId,
        int accountId,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        // Fake: accepts accountId but doesn't filter — auth tested at controller level
        var result = _relationships.Where(r =>
            r.campaign_id == campaignId &&
            (includeInactive || r.is_active));

        return Task.FromResult(result.AsEnumerable());
    }

    public Task UpdateRelationshipAsync(EntityRelationship relationship, CancellationToken ct = default)
    {
        var existing = _relationships.FirstOrDefault(r =>
            r.entity_relationship_id == relationship.entity_relationship_id);

        if (existing != null)
        {
            existing.description = relationship.description;
            existing.custom_type = relationship.custom_type;
            existing.strength = relationship.strength;
            existing.attitude_score = relationship.attitude_score;
            existing.is_active = relationship.is_active;
            existing.campaign_id = relationship.campaign_id;
            existing.updated_at = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }

    public Task DeleteRelationshipAsync(int relationshipId, CancellationToken ct = default)
    {
        var relationship = _relationships.FirstOrDefault(r => r.entity_relationship_id == relationshipId);
        if (relationship != null)
        {
            _relationships.Remove(relationship);
        }
        return Task.CompletedTask;
    }

    public Task DeactivateRelationshipAsync(int relationshipId, CancellationToken ct = default)
    {
        var relationship = _relationships.FirstOrDefault(r => r.entity_relationship_id == relationshipId);
        if (relationship != null)
        {
            relationship.is_active = false;
            relationship.updated_at = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task ReactivateRelationshipAsync(int relationshipId, CancellationToken ct = default)
    {
        var relationship = _relationships.FirstOrDefault(r => r.entity_relationship_id == relationshipId);
        if (relationship != null)
        {
            relationship.is_active = true;
            relationship.updated_at = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task<bool> RelationshipExistsAsync(
        string sourceEntityType,
        int sourceEntityId,
        string targetEntityType,
        int targetEntityId,
        int relationshipTypeId,
        CancellationToken ct = default)
    {
        var exists = _relationships.Any(r =>
            r.source_entity_type == sourceEntityType &&
            r.source_entity_id == sourceEntityId &&
            r.target_entity_type == targetEntityType &&
            r.target_entity_id == targetEntityId &&
            r.relationship_type_id == relationshipTypeId &&
            r.is_active);

        return Task.FromResult(exists);
    }

    public Task<IEnumerable<EntityRelationship>> GetAllRelationshipsOfAccountAsync(int accountId, bool includeInactive = false, CancellationToken ct = default)
    {
        return Task.FromResult(_relationships.AsEnumerable());
    }

    public Task<IEnumerable<EntityRelationship>> GetPcStancesForNpcAsync(
        int npcId,
        int accountId,
        int? campaignId = null,
        CancellationToken ct = default)
    {
        // Fake: accepts accountId but doesn't filter — auth tested at controller level
        var result = _relationships.Where(r =>
            r.is_active &&
            ((r.source_entity_type == "npc" && r.source_entity_id == npcId && r.target_entity_type == "pc") ||
             (r.target_entity_type == "npc" && r.target_entity_id == npcId && r.source_entity_type == "pc")) &&
            (!campaignId.HasValue || r.campaign_id == campaignId.Value));

        return Task.FromResult(result.AsEnumerable());
    }

    #endregion
}
