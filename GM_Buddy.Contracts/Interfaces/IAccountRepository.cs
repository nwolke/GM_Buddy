using GM_Buddy.Contracts.DbEntities;

namespace GM_Buddy.Contracts.Interfaces;

/// <summary>
/// Repository for account operations
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Get an account by its internal ID
    /// </summary>
    Task<Account?> GetByIdAsync(int accountId);
    
    /// <summary>
    /// Get an account by Cognito sub (user ID from JWT)
    /// </summary>
    Task<Account?> GetByCognitoSubAsync(string cognitoSub);
    
    /// <summary>
    /// Get an account by email
    /// </summary>
    Task<Account?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Create a new account for a Cognito user
    /// </summary>
    Task<Account> CreateAsync(string cognitoSub, string? email = null);
    
    /// <summary>
    /// Get or create an account for a Cognito user (upsert pattern).
    /// Only requires cognitoSub - email is optional.
    /// </summary>
    Task<Account> GetOrCreateByCognitoSubAsync(string cognitoSub, string? email = null);
    
    /// <summary>
    /// Update the subscription tier for an account
    /// </summary>
    Task UpdateSubscriptionTierAsync(int accountId, string tier);
    
    /// <summary>
    /// Update last login timestamp
    /// </summary>
    Task UpdateLastLoginAsync(int accountId);
}
