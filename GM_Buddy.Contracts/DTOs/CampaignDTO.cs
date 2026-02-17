namespace GM_Buddy.Contracts.DTOs;

public class CampaignDTO
{
    public int Campaign_id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
