namespace GM_Buddy.Contracts.DbEntities;

public class Npc
{
    public int npc_id { get; set; }
    public int user_id { get; set; }

    // FK ids
    public required int game_system_id { get; set; }
    public required int lineage_id { get; set; }
    public required int occupation_id { get; set; }

    // Core fields
    public required string name { get; set; }
    public required string stats { get; set; }
    public required string description { get; set; }
    public required string gender { get; set; }

    // Denormalized / joined display names (nullable because joins may not populate them)
    public string? lineage_name { get; set; }
    public string? occupation_name { get; set; }
    public string? game_system_name { get; set; }
}