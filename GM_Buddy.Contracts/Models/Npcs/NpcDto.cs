namespace GM_Buddy.Contracts.Models.Npcs;

public class NpcDto
{
    public int? Npc_Id { get; set; }
    public int Account_Id { get; set; }
    public int Campaign_Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Lineage { get; set; }
    public string? Class { get; set; }
    public string? Faction { get; set; }
    public string? Notes { get; set; }
}
