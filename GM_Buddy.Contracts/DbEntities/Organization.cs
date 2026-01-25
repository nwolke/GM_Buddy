namespace GM_Buddy.Contracts.DbEntities;

/// <summary>
/// Represents an organization from the public.organization table
/// </summary>
public class Organization
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int organization_id { get; set; }

    /// <summary>
    /// Foreign key to auth.account
    /// </summary>
    public int account_id { get; set; }

    /// <summary>
    /// Organization name
    /// </summary>
    public required string name { get; set; }

    /// <summary>
    /// Organization description
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
}
