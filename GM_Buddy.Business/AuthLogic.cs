using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.Extensions.Logging;

namespace GM_Buddy.Business;

public class AuthLogic : IAuthLogic
{
    private readonly ILogger<AuthLogic> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly INewAccountDataSeeder _newAccountDataSeeder;

    public AuthLogic(ILogger<AuthLogic> logger, 
        IAccountRepository accountRepository,
        INewAccountDataSeeder newAccountDataSeeder)
    {
        _logger = logger;
        _accountRepository = accountRepository;
        _newAccountDataSeeder = newAccountDataSeeder;
    }

    public async Task<Account> GetOrCreateAccountByCognitoSubAsync(string cognitoSub, string? email)
    {
        // Try to get existing account
        var account = await _accountRepository.GetByCognitoSubAsync(cognitoSub);
        if (account != null)
        {
            _logger.LogInformation("Account {id} found for cognito sub {cognitoSub}", account.account_id, cognitoSub);
            
            // Update last login asynchronously without waiting (fire-and-forget)
            // This prevents the login flow from waiting for the UPDATE to complete
            _ = Task.Run(async () =>
            {
                try
                {
                    await _accountRepository.UpdateLastLoginAsync(account.account_id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update last login for account {AccountId}", account.account_id);
                }
            });
            
            return account;
        }

        // Check if account exists with same email but no cognito_sub (legacy account)
        if (!string.IsNullOrEmpty(email))
        {
            account = await _accountRepository.GetByEmailAsync(email);
            if (account != null)
            {
                if (string.IsNullOrEmpty(account.cognito_sub))
                {
                    _logger.LogInformation("Account {id} is legacy and has no cognito sub. Updating.", account.account_id);
                    // Link existing account to Cognito
                    return await _accountRepository.UpdateCognitoSubForAccount(cognitoSub, account);
                }

                _logger.LogWarning("Account {id} with email {email} already has a cognito_sub assigned; skipping legacy link.", account.account_id, email);
            }
        }

        _logger.LogInformation("New Account being created for email: {email}", email);
        // Create new account
        var newAccount = await _accountRepository.CreateAsync(cognitoSub, email);
        // seed new account data with defaults
        await _newAccountDataSeeder.SeedDefaultDataForNewAccountAsync(newAccount.account_id);
        return newAccount;
    }

}
