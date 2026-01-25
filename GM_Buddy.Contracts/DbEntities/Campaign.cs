namespace GM_Buddy.Contracts.DbEntities;

/// <summary>
/// Represents a campaign from the public.campaign table
/// </summary>
public class Campaign
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int campaign_id { get; set; }

    /// <summary>
    /// Foreign key to auth.account
    /// </summary>
    public int account_id { get; set; }

    /// <summary>
    /// Foreign key to game_system
    /// </summary>
    public int game_system_id { get; set; }

    /// <summary>
    /// Campaign name
    /// </summary>
    public required string name { get; set; }

    /// <summary>
    /// Campaign description
    /// </summary>
    public string? description { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime created_at { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime updated_at { get; set; }

    /// <summary>
    /// Game system name (from join)
    /// </summary>
    public string? game_system_name { get; set; }
}
