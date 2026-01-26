namespace GM_Buddy.Contracts.DbEntities;

/// <summary>
/// Represents a polymorphic entity relationship from the public.entity_relationship table
/// Tracks relationships between NPCs, PCs, and Organizations
/// </summary>
public class EntityRelationship
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int entity_relationship_id { get; set; }

    /// <summary>
    /// Source entity type ('npc', 'pc', or 'organization')
    /// </summary>
    public required string source_entity_type { get; set; }

    /// <summary>
    /// Source entity ID
    /// </summary>
    public int source_entity_id { get; set; }

    /// <summary>
    /// Target entity type ('npc', 'pc', or 'organization')
    /// </summary>
    public required string target_entity_type { get; set; }

    /// <summary>
    /// Target entity ID
    /// </summary>
    public int target_entity_id { get; set; }

    /// <summary>
    /// Foreign key to relationship_type
    /// </summary>
    public int relationship_type_id { get; set; }

    /// <summary>
    /// Optional description/notes for this specific relationship
    /// </summary>
    public string? description { get; set; }

    /// <summary>
    /// Relationship strength/importance (1-10, where 10 is strongest)
    /// </summary>
    public int? strength { get; set; }

    /// <summary>
    /// Whether the relationship is currently active
    /// </summary>
    public bool is_active { get; set; } = true;

    /// <summary>
    /// Optional: Campaign context for this relationship
    /// </summary>
    public int? campaign_id { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime created_at { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime updated_at { get; set; }

    // Navigation/computed properties (populated from joins)

    /// <summary>
    /// Source entity name (from join)
    /// </summary>
    public string? source_name { get; set; }

    /// <summary>
    /// Target entity name (from join)
    /// </summary>
    public string? target_name { get; set; }

    /// <summary>
    /// Relationship type name (from join)
    /// </summary>
    public string? relationship_type_name { get; set; }

    /// <summary>
    /// Campaign name (from join)
    /// </summary>
    public string? campaign_name { get; set; }

    /// <summary>
    /// Whether the relationship type is directional (from join)
    /// </summary>
    public bool? is_directional { get; set; }
}
