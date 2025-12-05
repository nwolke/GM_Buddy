namespace GM_Buddy.Contracts.DbEntities;

/// <summary>
/// Represents a relationship type from the public.relationship_type table
/// </summary>
public class RelationshipType
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int relationship_type_id { get; set; }

    /// <summary>
    /// Name of the relationship type (e.g., Friend, Enemy, Mentor)
    /// </summary>
    public required string relationship_type_name { get; set; }

    /// <summary>
    /// Description of the relationship type
    /// </summary>
    public string? description { get; set; }

    /// <summary>
    /// Whether the relationship has directionality (e.g., Parent vs Sibling)
    /// </summary>
    public bool is_directional { get; set; } = true;

    /// <summary>
    /// Foreign key to the inverse relationship type (e.g., Parent <-> Child)
    /// </summary>
    public int? inverse_type_id { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime created_at { get; set; }

    /// <summary>
    /// Name of the inverse relationship type (from join)
    /// </summary>
    public string? inverse_type_name { get; set; }
}
