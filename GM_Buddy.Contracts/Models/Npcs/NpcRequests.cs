namespace GM_Buddy.Contracts.Models.Npcs;

/// <summary>
/// Request model for creating a new NPC
/// </summary>
public class CreateNpcRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? System { get; set; }
    public string? Race { get; set; }
    public string? Class { get; set; }
    public string? Faction { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request model for updating an existing NPC
/// </summary>
public class UpdateNpcRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? System { get; set; }
    public string? Race { get; set; }
    public string? Class { get; set; }
    public string? Faction { get; set; }
    public string? Notes { get; set; }
}
