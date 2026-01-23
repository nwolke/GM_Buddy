namespace GM_Buddy.Contracts.DbEntities;

/// <summary>
/// Represents a user account from the auth.account table.
/// Links Cognito users to internal data and tracks subscription status.
/// </summary>
public class Account
{
    /// <summary>
    /// Primary key (maps to auth.account.id)
    /// </summary>
    public int account_id { get; set; }

    /// <summary>
    /// Unique username for login (optional with Cognito)
    /// </summary>
    public string? username { get; set; }
    
    /// <summary>
    /// User's first name
    /// </summary>
    public string? first_name { get; set; }
 
    /// <summary>
    /// User's last name (optional)
    /// </summary>
    public string? last_name { get; set; }
    
    /// <summary>
    /// Email address (unique, used for authentication)
    /// </summary>
    public required string email { get; set; }
    
    /// <summary>
    /// Cognito user ID (sub claim from JWT)
    /// </summary>
    public string? cognito_sub { get; set; }
    
    /// <summary>
    /// Subscription tier: free, supporter, premium, lifetime
    /// </summary>
    public string subscription_tier { get; set; } = "free";
    
    /// <summary>
    /// Account creation timestamp
    /// </summary>
    public DateTime created_at { get; set; }
    
    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? last_login_at { get; set; }
    
    /// <summary>
    /// Display name (computed from first and last name, or email)
    /// </summary>
    public string DisplayName => !string.IsNullOrWhiteSpace(first_name) 
        ? $"{first_name} {last_name}".Trim() 
        : email;
}
