namespace GM_Buddy.Contracts.DbEntities;

public class Npc
{
    public int npc_id { get; set; }
    public int account_id { get; set; }
        // FK ids
    public required int game_system_id { get; set; }
    // Core fields
    public required string stats { get; set; }

    public string? game_system_name { get; set; }
}