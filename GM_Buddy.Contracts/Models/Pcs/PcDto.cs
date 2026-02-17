namespace GM_Buddy.Contracts.Models.Pcs;

/// <summary>
/// Data Transfer Object for Player Character
/// Excludes sensitive properties like account_id
/// </summary>
public class PcDto
{
    /// <summary>
    /// PC ID
    /// </summary>
    public int PcId { get; set; }

    /// <summary>
    /// PC name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// PC description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
