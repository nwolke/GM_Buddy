namespace GM_Buddy.Contracts.DbEntities;

public class ReferenceLineage
{
    public int lineage_id { get; set; }
    public int game_system_id { get; set; }
    public int? account_id { get; set; }
    public int? campaign_id { get; set; }
    public string name { get; set; } = string.Empty;
    public string? description { get; set; }
    public bool is_active { get; set; } = true;
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    
    public bool IsGlobal => account_id == null && campaign_id == null;
}
