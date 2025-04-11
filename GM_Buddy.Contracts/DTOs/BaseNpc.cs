namespace GM_Buddy.Contracts.DTOs;

public class BaseNpc
{
    public int Npc_Id { get; set; }
    public int UserId { get; set; }
    public string? Name { get; set; }
    public string? Lineage { get; set; }
    public string? Occupation { get; set; }
    public string? System { get; set; }
    public string? Description { get; set; }
    public string? Gender { get; set; }
}
