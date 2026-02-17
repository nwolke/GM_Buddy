namespace GM_Buddy.Contracts.Models.Pcs;

/// <summary>
/// Data Transfer Object for a Player Character response.
/// account_id is intentionally omitted to prevent exposure to clients.
/// </summary>
public class PcDto
{
    public int Pc_Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
