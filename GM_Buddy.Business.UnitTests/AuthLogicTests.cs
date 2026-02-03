using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;

namespace GM_Buddy.Business.UnitTests;

/// <summary>
/// Unit tests for AuthLogic class
/// </summary>
public class AuthLogicTests
{
    [Fact]
    public async Task GetOrCreateAccountByCognitoSubAsync_ExistingAccount_ReturnsAccountAndUpdatesLastLogin()
    {
        // Arrange
        var existingAccount = new Account
        {
            account_id = 1,
            cognito_sub = "cognito-sub-123",
            email = "existing@example.com",
            subscription_tier = "free",
            created_at = DateTime.UtcNow.AddDays(-30),
            last_login_at = DateTime.UtcNow.AddDays(-5)
        };

        var accountRepo = new FakeAccountRepository(new[] { existingAccount });
        var seeder = new FakeNewAccountDataSeeder();
        var authLogic = new AuthLogic(
            NullLogger<AuthLogic>.Instance,
            accountRepo,
            seeder
        );

        var originalLastLogin = existingAccount.last_login_at;

        // Act
        var result = await authLogic.GetOrCreateAccountByCognitoSubAsync("cognito-sub-123", "existing@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.account_id);
        Assert.Equal("cognito-sub-123", result.cognito_sub);
        
        // Note: last_login_at update is now async/fire-and-forget for performance
        // The returned account will have the old last_login_at value for immediate response speed
        Assert.False(seeder.WasCalled, "Seeder should not be called for existing account");
        
        // Give the background task time to complete
        await Task.Delay(500);
        
        // Verify the update happened in the repository
        var updatedAccount = await accountRepo.GetByIdAsync(1);
        Assert.NotNull(updatedAccount);
        Assert.True(updatedAccount.last_login_at > originalLastLogin, "Last login should be updated in background");
    }

    [Fact]
    public async Task GetOrCreateAccountByCognitoSubAsync_NewAccount_CreatesAccountAndSeedsData()
    {
        // Arrange
        var accountRepo = new FakeAccountRepository();
        var seeder = new FakeNewAccountDataSeeder();
        var authLogic = new AuthLogic(
            NullLogger<AuthLogic>.Instance,
            accountRepo,
            seeder
        );

        // Act
        var result = await authLogic.GetOrCreateAccountByCognitoSubAsync("new-cognito-sub", "newuser@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new-cognito-sub", result.cognito_sub);
        Assert.Equal("newuser@example.com", result.email);
        Assert.Equal("free", result.subscription_tier);
        Assert.True(seeder.WasCalled, "Seeder should be called for new account");
        Assert.Equal(result.account_id, seeder.SeededAccountId);
    }

    [Fact]
    public async Task GetOrCreateAccountByCognitoSubAsync_LegacyAccountWithEmail_LinksToCognitoAndDoesNotSeed()
    {
        // Arrange
        var legacyAccount = new Account
        {
            account_id = 5,
            cognito_sub = null, // Legacy account has no cognito_sub
            email = "legacy@example.com",
            subscription_tier = "premium",
            created_at = DateTime.UtcNow.AddYears(-1),
            last_login_at = DateTime.UtcNow.AddMonths(-2)
        };

        var accountRepo = new FakeAccountRepository(new[] { legacyAccount });
        var seeder = new FakeNewAccountDataSeeder();
        var authLogic = new AuthLogic(
            NullLogger<AuthLogic>.Instance,
            accountRepo,
            seeder
        );

        // Act
        var result = await authLogic.GetOrCreateAccountByCognitoSubAsync("new-cognito-sub", "legacy@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.account_id);
        Assert.Equal("new-cognito-sub", result.cognito_sub);
        Assert.Equal("legacy@example.com", result.email);
        Assert.Equal("premium", result.subscription_tier); // Should preserve existing tier
        Assert.False(seeder.WasCalled, "Seeder should not be called for legacy account migration");
    }

    [Fact]
    public async Task GetOrCreateAccountByCognitoSubAsync_EmptyEmail_CreatesNewAccount()
    {
        // Arrange
        var accountRepo = new FakeAccountRepository();
        var seeder = new FakeNewAccountDataSeeder();
        var authLogic = new AuthLogic(
            NullLogger<AuthLogic>.Instance,
            accountRepo,
            seeder
        );

        // Act
        var result = await authLogic.GetOrCreateAccountByCognitoSubAsync("cognito-sub-456", "");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("cognito-sub-456", result.cognito_sub);
        Assert.True(seeder.WasCalled);
    }

    [Fact]
    public async Task GetOrCreateAccountByCognitoSubAsync_NullEmail_CreatesNewAccount()
    {
        // Arrange
        var accountRepo = new FakeAccountRepository();
        var seeder = new FakeNewAccountDataSeeder();
        var authLogic = new AuthLogic(
            NullLogger<AuthLogic>.Instance,
            accountRepo,
            seeder
        );

        // Act
        var result = await authLogic.GetOrCreateAccountByCognitoSubAsync("cognito-sub-789", null!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("cognito-sub-789", result.cognito_sub);
        Assert.True(seeder.WasCalled);
    }
}

/// <summary>
/// Fake implementation of INewAccountDataSeeder for testing
/// </summary>
internal class FakeNewAccountDataSeeder : INewAccountDataSeeder
{
    public bool WasCalled { get; private set; }
    public int SeededAccountId { get; private set; }

    public Task SeedDefaultDataForNewAccountAsync(int accountId)
    {
        WasCalled = true;
        SeededAccountId = accountId;
        return Task.CompletedTask;
    }
}
