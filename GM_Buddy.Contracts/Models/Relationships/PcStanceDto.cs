namespace GM_Buddy.Contracts.Models.Relationships;

/// <summary>
/// Represents a single PC's relationship stance toward an NPC.
/// Used by the PC-stance grid in the relationship manager.
/// </summary>
public class PcStanceDto
{
    /// <summary>
    /// The entity_relationship_id for this stance
    /// </summary>
    public int Entity_Relationship_Id { get; set; }

    /// <summary>
    /// The PC's ID
    /// </summary>
    public int Pc_Id { get; set; }

    /// <summary>
    /// The PC's display name
    /// </summary>
    public required string Pc_Name { get; set; }

    /// <summary>
    /// The relationship type name (e.g., "Ally", "Enemy")
    /// </summary>
    public string? Relationship_Type { get; set; }

    /// <summary>
    /// Relationship type ID (needed for inline editing)
    /// </summary>
    public int Relationship_Type_Id { get; set; }

    /// <summary>
    /// Disposition score (-5 to +5), null if unset
    /// </summary>
    public int? Disposition { get; set; }

    /// <summary>
    /// Optional description/notes for this specific stance
    /// </summary>
    public string? Description { get; set; }
}
