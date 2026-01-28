namespace GM_Buddy.Contracts.DbEntities;

public class Npc
{
    public int npc_id { get; set; }
    public int account_id { get; set; }
    public required int campaign_id { get; set; }
    // Core fields
    public required string name { get; set; }
    public string? description { get; set; }
    public required string stats { get; set; }
    public int? game_system_id { get; set; }
    public string? game_system_name { get; set; }
}