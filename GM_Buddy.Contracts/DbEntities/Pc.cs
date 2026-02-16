namespace GM_Buddy.Contracts.DbEntities;

/// <summary>
/// Represents a player character from the public.pc table
/// </summary>
public class Pc
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int pc_id { get; set; }

    /// <summary>
    /// Foreign key to auth.account
    /// </summary>
    public int account_id { get; set; }

    /// <summary>
    /// PC name
    /// </summary>
    public required string name { get; set; }

    /// <summary>
    /// PC description
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
