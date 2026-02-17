namespace GM_Buddy.Contracts.Models.Pcs;

/// <summary>
/// Request model for creating a new PC
/// </summary>
public class CreatePcRequest
{
    /// <summary>
    /// PC name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// PC description
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Request model for updating an existing PC
/// </summary>
public class UpdatePcRequest
{
    /// <summary>
    /// PC name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// PC description
    /// </summary>
    public string? Description { get; set; }
}
