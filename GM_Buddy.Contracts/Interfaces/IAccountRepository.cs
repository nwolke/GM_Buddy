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
    /// Link an existing legacy account (without a Cognito sub) to a Cognito user
    /// by updating the account's cognito_sub field with the provided Cognito user ID.
    /// Does not create a new account.
    /// </summary>
    Task<Account> UpdateCognitoSubForAccount(string cognitoSub, Account account);

    /// <summary>
    /// Update the subscription tier for an account
    /// </summary>
    Task UpdateSubscriptionTierAsync(int accountId, string tier);
    
    /// <summary>
    /// Update last login timestamp
    /// </summary>
    Task UpdateLastLoginAsync(int accountId);

    /// <summary>
    /// Delete an account and all associated data (cascades via database constraints)
    /// </summary>
    Task DeleteAsync(int accountId);
}
