using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace GM_Buddy.Business.UnitTests;

/// <summary>
/// Unit tests for AccountLogic class
/// </summary>
public class AccountLogicTests
{
    [Fact]
    public async Task DeleteAccountAsync_ExistingAccount_DeletesSuccessfully()
    {
        // Arrange
        var account = new Account
        {
            account_id = 1,
            cognito_sub = "cognito-123",
            email = "test@example.com",
            subscription_tier = "free"
        };

        var accountRepo = new FakeAccountRepository(new[] { account });
        var campaignRepo = new FakeCampaignRepositoryForAccountTests();
        var npcRepo = new FakeNpcRepositoryForAccountTests();
        var pcRepo = new FakePcRepository();
        var orgRepo = new FakeOrganizationRepository();
        var relRepo = new FakeRelationshipRepository();

        var accountLogic = new AccountLogic(
            NullLogger<AccountLogic>.Instance,
            accountRepo,
            campaignRepo,
            npcRepo,
            pcRepo,
            orgRepo,
            relRepo
        );

        // Act
        await accountLogic.DeleteAccountAsync(1);

        // Assert
        var deletedAccount = await accountRepo.GetByIdAsync(1);
        Assert.Null(deletedAccount);
    }

    [Fact]
    public async Task DeleteAccountAsync_NonExistentAccount_ThrowsException()
    {
        // Arrange
        var accountRepo = new FakeAccountRepository();
        var campaignRepo = new FakeCampaignRepositoryForAccountTests();
        var npcRepo = new FakeNpcRepositoryForAccountTests();
        var pcRepo = new FakePcRepository();
        var orgRepo = new FakeOrganizationRepository();
        var relRepo = new FakeRelationshipRepository();

        var accountLogic = new AccountLogic(
            NullLogger<AccountLogic>.Instance,
            accountRepo,
            campaignRepo,
            npcRepo,
            pcRepo,
            orgRepo,
            relRepo
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await accountLogic.DeleteAccountAsync(999)
        );
    }

    [Fact]
    public async Task ExportAccountDataAsync_ExistingAccount_ReturnsAllData()
    {
        // Arrange
        var account = new Account
        {
            account_id = 1,
            cognito_sub = "cognito-123",
            email = "test@example.com",
            username = "testuser",
            subscription_tier = "free",
            created_at = DateTime.UtcNow.AddMonths(-6)
        };

        var accountRepo = new FakeAccountRepository(new[] { account });
        var campaignRepo = new FakeCampaignRepositoryForAccountTests();
        var npcRepo = new FakeNpcRepositoryForAccountTests();
        var pcRepo = new FakePcRepository();
        var orgRepo = new FakeOrganizationRepository();
        var relRepo = new FakeRelationshipRepository();

        var accountLogic = new AccountLogic(
            NullLogger<AccountLogic>.Instance,
            accountRepo,
            campaignRepo,
            npcRepo,
            pcRepo,
            orgRepo,
            relRepo
        );

        // Act
        var result = await accountLogic.ExportAccountDataAsync(1);

        // Assert
        Assert.NotNull(result);
        
        // Serialize to JSON and deserialize to verify structure
        var json = JsonSerializer.Serialize(result);
        var doc = JsonDocument.Parse(json);
        
        // Verify metadata
        Assert.True(doc.RootElement.TryGetProperty("ExportMetadata", out var metadata));
        Assert.Equal(1, metadata.GetProperty("AccountId").GetInt32());
        Assert.Equal("1.0", metadata.GetProperty("Version").GetString());
        
        // Verify account data
        Assert.True(doc.RootElement.TryGetProperty("Account", out var accountData));
        Assert.Equal("test@example.com", accountData.GetProperty("email").GetString());
        Assert.Equal(1, accountData.GetProperty("account_id").GetInt32());
    }

    [Fact]
    public async Task ExportAccountDataAsync_NonExistentAccount_ThrowsException()
    {
        // Arrange
        var accountRepo = new FakeAccountRepository();
        var campaignRepo = new FakeCampaignRepositoryForAccountTests();
        var npcRepo = new FakeNpcRepositoryForAccountTests();
        var pcRepo = new FakePcRepository();
        var orgRepo = new FakeOrganizationRepository();
        var relRepo = new FakeRelationshipRepository();

        var accountLogic = new AccountLogic(
            NullLogger<AccountLogic>.Instance,
            accountRepo,
            campaignRepo,
            npcRepo,
            pcRepo,
            orgRepo,
            relRepo
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await accountLogic.ExportAccountDataAsync(999)
        );
    }

    [Fact]
    public async Task ExportAccountDataAsync_AccountWithMultipleEntities_IncludesAllEntities()
    {
        // Arrange
        var account = new Account
        {
            account_id = 10,
            cognito_sub = "cognito-456",
            email = "user@example.com",
            subscription_tier = "premium"
        };

        var accountRepo = new FakeAccountRepository(new[] { account });
        var campaignRepo = new FakeCampaignRepositoryForAccountTests();
        var npcRepo = new FakeNpcRepositoryForAccountTests();
        var pcRepo = new FakePcRepository();
        var orgRepo = new FakeOrganizationRepository();
        var relRepo = new FakeRelationshipRepository();

        var accountLogic = new AccountLogic(
            NullLogger<AccountLogic>.Instance,
            accountRepo,
            campaignRepo,
            npcRepo,
            pcRepo,
            orgRepo,
            relRepo
        );

        // Act
        var result = await accountLogic.ExportAccountDataAsync(10);

        // Assert
        Assert.NotNull(result);
        
        // Serialize to JSON and verify structure
        var json = JsonSerializer.Serialize(result);
        var doc = JsonDocument.Parse(json);
        
        // Verify campaigns array exists and has data
        Assert.True(doc.RootElement.TryGetProperty("Campaigns", out var campaigns));
        Assert.Equal(JsonValueKind.Array, campaigns.ValueKind);
        Assert.True(campaigns.GetArrayLength() > 0, "Should have at least one campaign");
        
        // Verify NPCs array exists and has data
        Assert.True(doc.RootElement.TryGetProperty("NPCs", out var npcs));
        Assert.Equal(JsonValueKind.Array, npcs.ValueKind);
        Assert.True(npcs.GetArrayLength() > 0, "Should have at least one NPC");
        
        // Verify other entity arrays exist
        Assert.True(doc.RootElement.TryGetProperty("PCs", out _));
        Assert.True(doc.RootElement.TryGetProperty("Organizations", out _));
        Assert.True(doc.RootElement.TryGetProperty("Relationships", out _));
    }
}

/// <summary>
/// Simplified fake campaign repository for account logic tests
/// </summary>
internal class FakeCampaignRepositoryForAccountTests : ICampaignRepository
{
    private readonly List<Campaign> _campaigns = new()
    {
        new Campaign { campaign_id = 1, account_id = 10, game_system_id = 1, name = "Test Campaign 1" },
        new Campaign { campaign_id = 2, account_id = 10, game_system_id = 2, name = "Test Campaign 2" },
    };

    public Task<IEnumerable<Campaign>> GetByAccountIdAsync(int accountId, CancellationToken ct = default)
    {
        return Task.FromResult(_campaigns.Where(c => c.account_id == accountId).AsEnumerable());
    }

    public Task<Campaign?> GetByIdAsync(int campaignId, CancellationToken ct = default)
    {
        return Task.FromResult(_campaigns.FirstOrDefault(c => c.campaign_id == campaignId));
    }

    public Task<Campaign?> GetByIdAndAccountAsync(int campaignId, int accountId, CancellationToken ct = default)
    {
        return Task.FromResult(_campaigns.FirstOrDefault(c => c.campaign_id == campaignId && c.account_id == accountId));
    }

    public Task<int> CreateAsync(Campaign campaign, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int campaignId, int accountId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(Campaign campaign, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Simplified fake NPC repository for account logic tests
/// </summary>
internal class FakeNpcRepositoryForAccountTests : INpcRepository
{
    private readonly List<Npc> _npcs = new()
    {
        new Npc { npc_id = 1, account_id = 10, campaign_id = 1, name = "Test NPC 1", stats = "{}" },
        new Npc { npc_id = 2, account_id = 10, campaign_id = 1, name = "Test NPC 2", stats = "{}" },
    };

    public Task<IEnumerable<Npc>> GetNpcs(int accountId, int? campaignId, CancellationToken ct = default)
    {
        var query = _npcs.Where(n => n.account_id == accountId);
        if (campaignId.HasValue)
        {
            query = query.Where(n => n.campaign_id == campaignId.Value);
        }
        return Task.FromResult(query.AsEnumerable());
    }

    public Task<Npc?> GetNpcById(int npcId, CancellationToken ct = default)
    {
        return Task.FromResult(_npcs.FirstOrDefault(n => n.npc_id == npcId));
    }

    public Task<int> CreateNpcAsync(Npc npc, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateNpcAsync(Npc npc, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteNpcAsync(int npcId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
