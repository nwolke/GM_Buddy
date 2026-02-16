namespace GM_Buddy.Contracts.DTOs;

public class CampaignDTO
{
    public int Campaign_id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? Game_system_id { get; set; }
    public string? Game_system_name { get; set; }
}
