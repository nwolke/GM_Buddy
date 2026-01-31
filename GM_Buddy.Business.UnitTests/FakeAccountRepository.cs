using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;

namespace GM_Buddy.Business.UnitTests;

/// <summary>
/// Fake in-memory implementation of IAccountRepository for testing
/// </summary>
internal class FakeAccountRepository : IAccountRepository
{
    private readonly List<Account> _accounts;
    private int _nextId = 1;

    public FakeAccountRepository(IEnumerable<Account>? accounts = null)
    {
        _accounts = accounts?.ToList() ?? new List<Account>();
        if (_accounts.Any())
        {
            _nextId = _accounts.Max(a => a.account_id) + 1;
        }
    }

    public Task<Account?> GetByIdAsync(int accountId)
    {
        var account = _accounts.FirstOrDefault(a => a.account_id == accountId);
        return Task.FromResult(account);
    }

    public Task<Account?> GetByCognitoSubAsync(string cognitoSub)
    {
        var account = _accounts.FirstOrDefault(a => a.cognito_sub == cognitoSub);
        return Task.FromResult(account);
    }

    public Task<Account?> GetByEmailAsync(string email)
    {
        var account = _accounts.FirstOrDefault(a => a.email == email);
        return Task.FromResult(account);
    }

    public Task<Account> CreateAsync(string cognitoSub, string? email = null)
    {
        var newAccount = new Account
        {
            account_id = _nextId++,
            cognito_sub = cognitoSub,
            email = email,
            subscription_tier = "free",
            created_at = DateTime.UtcNow,
            last_login_at = DateTime.UtcNow
        };
        _accounts.Add(newAccount);
        return Task.FromResult(newAccount);
    }

    public Task<Account> UpdateCognitoSubForAccount(string cognitoSub, Account account)
    {
        var existing = _accounts.FirstOrDefault(a => a.account_id == account.account_id);
        if (existing != null)
        {
            existing.cognito_sub = cognitoSub;
            existing.last_login_at = DateTime.UtcNow;
            return Task.FromResult(existing);
        }
        throw new InvalidOperationException($"Account {account.account_id} not found");
    }

    public Task UpdateSubscriptionTierAsync(int accountId, string tier)
    {
        var account = _accounts.FirstOrDefault(a => a.account_id == accountId);
        if (account != null)
        {
            account.subscription_tier = tier;
        }
        return Task.CompletedTask;
    }

    public Task UpdateLastLoginAsync(int accountId)
    {
        var account = _accounts.FirstOrDefault(a => a.account_id == accountId);
        if (account != null)
        {
            account.last_login_at = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int accountId)
    {
        var account = _accounts.FirstOrDefault(a => a.account_id == accountId);
        if (account != null)
        {
            _accounts.Remove(account);
        }
        return Task.CompletedTask;
    }
}
