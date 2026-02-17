namespace GM_Buddy.Contracts.Models.Npcs;

public class BaseNpc
{
    public int? Npc_Id { get; set; }
    public int Account_Id { get; set; }
    public int Campaign_Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
