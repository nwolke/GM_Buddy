using System.ComponentModel.DataAnnotations;

namespace GM_Buddy.Contracts.Models.Pcs;

/// <summary>
/// Request DTO for creating a new Player Character.
/// </summary>
public class CreatePcRequest
{
    [Required]
    public required string Name { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Request DTO for updating an existing Player Character.
/// </summary>
public class UpdatePcRequest
{
    [Required]
    public required string Name { get; set; }
    public string? Description { get; set; }
}
