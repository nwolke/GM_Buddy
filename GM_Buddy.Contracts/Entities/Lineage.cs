﻿namespace GM_Buddy.Contracts.Entities;

public class Lineage
{
    public int lineage_id { get; set; }
    public int game_system_id { get; set; }
    public required string lineage_name { get; set; }
}
