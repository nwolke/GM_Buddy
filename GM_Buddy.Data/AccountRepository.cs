using Dapper;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using Npgsql;
using System.Security.Principal;

namespace GM_Buddy.Data;

/// <summary>
/// Repository for account operations using Dapper
/// </summary>
public class AccountRepository : IAccountRepository
{
    private readonly string _connectionString;

    public AccountRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Account?> GetByIdAsync(int accountId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<Account>(
            @"SELECT id as account_id, username, first_name, last_name, email, 
                     cognito_sub, subscription_tier, created_at, last_login_at
              FROM auth.account 
              WHERE id = @accountId",
            new { accountId });
    }

    public async Task<Account?> GetByCognitoSubAsync(string cognitoSub)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<Account>(
            @"SELECT id as account_id, username, first_name, last_name, email, 
                     cognito_sub, subscription_tier, created_at, last_login_at
              FROM auth.account 
              WHERE cognito_sub = @cognitoSub",
            new { cognitoSub });
    }

    public async Task<Account?> GetByEmailAsync(string email)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<Account>(
            @"SELECT id as account_id, username, first_name, last_name, email, 
                     cognito_sub, subscription_tier, created_at, last_login_at
              FROM auth.account 
              WHERE email = @email",
            new { email });
    }

    public async Task<Account> CreateAsync(string cognitoSub, string? email = null)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        
        // Use cognitoSub as email placeholder if email not provided
        var emailValue = email ?? $"{cognitoSub}@cognito.user";
        
        var accountId = await connection.QuerySingleAsync<int>(
            @"INSERT INTO auth.account (cognito_sub, email, subscription_tier, created_at)
              VALUES (@cognitoSub, @emailValue, 'free', NOW())
              RETURNING id",
            new { cognitoSub, emailValue });

        return (await GetByIdAsync(accountId))!;
    }

    public async Task<Account> UpdateCognitoSubForAccount(string cognitoSub, Account account)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"UPDATE auth.account 
                      SET cognito_sub = @cognitoSub, last_login_at = NOW()
                      WHERE id = @accountId",
            new { cognitoSub, accountId = account.account_id });

        account.cognito_sub = cognitoSub;
        return account;
    }

    public async Task UpdateSubscriptionTierAsync(int accountId, string tier)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"UPDATE auth.account 
              SET subscription_tier = @tier
              WHERE id = @accountId",
            new { accountId, tier });
    }

    public async Task UpdateLastLoginAsync(int accountId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"UPDATE auth.account 
              SET last_login_at = NOW()
              WHERE id = @accountId",
            new { accountId });
    }
}
