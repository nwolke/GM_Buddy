namespace GM_Buddy.Contracts.DbEntities;

/// <summary>
/// Represents a user account from the auth.account table
/// </summary>
public class Account
{
    /// <summary>
    /// Primary key (maps to auth.account.id)
    /// </summary>
    public int account_id { get; set; }

 /// <summary>
    /// Unique username for login
    /// </summary>
    public string? username { get; set; }
    
    /// <summary>
    /// User's first name
    /// </summary>
    public required string first_name { get; set; }
 
    /// <summary>
    /// User's last name (optional)
    /// </summary>
    public string? last_name { get; set; }
    
    /// <summary>
    /// Email address (unique, used for authentication)
    /// </summary>
    public required string email { get; set; }
    
    /// <summary>
    /// Hashed password (do not expose in API responses)
    /// </summary>
    public string? password { get; set; }
    
    /// <summary>
    /// Password salt (do not expose in API responses)
    /// </summary>
    public string? salt { get; set; }
    
    /// <summary>
    /// Account creation timestamp
    /// </summary>
    public DateTime created_at { get; set; }
    
    /// <summary>
/// Display name (computed from first and last name)
/// </summary>
    public string FullName => $"{first_name} {last_name}".Trim();
}
