namespace GM_Buddy.Contracts.DbEntities;

public class Npc
{
    public int npc_id { get; set; }
    public int account_id { get; set; }
    public required int campaign_id { get; set; }
    // Core fields
    public required string name { get; set; }
    public string? description { get; set; }
    // Flattened properties (moved from stats object)
    public string? lineage { get; set; }
    public string? @class { get; set; }
    public string? faction { get; set; }
    public string? notes { get; set; }
}