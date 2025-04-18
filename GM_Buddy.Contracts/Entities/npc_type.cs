namespace GM_Buddy.Contracts.Entities;

public class npc_type
{
    public int npc_id { get; set; }
    public int user_id { get; set; }
    public required string game_system_name { get; set; }
    public required string lineage_name { get; set; }
    public required string occupation_name { get; set; }
    public required string name { get; set; }
    public required string stats { get; set; }
    public required string description { get; set; }
    public required string gender { get; set; }
}