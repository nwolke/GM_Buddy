using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;

namespace GM_Buddy.Business.UnitTests;

internal class FakeNpcRepository : INpcRepository
{
    private readonly List<Npc> _npcs;
    private int _nextId = 100;

    public FakeNpcRepository(IEnumerable<Npc>? npcs = null)
    {
        _npcs = npcs?.ToList() ?? new List<Npc>();
    }

    public Task<IEnumerable<Npc>> GetNpcs(int accountId, int? campaignId, CancellationToken ct = default)
    {
        var result = _npcs.Where(n => n.account_id == accountId);
        if (campaignId.HasValue)
        {
            result = result.Where(n => n.campaign_id == campaignId.Value);
        }
        return Task.FromResult(result.AsEnumerable());
    }

    public Task<Npc?> GetNpcById(int npcId, CancellationToken ct = default)
    {
        var npc = _npcs.FirstOrDefault(n => n.npc_id == npcId);
        return Task.FromResult(npc);
    }

    public Task<int> CreateNpcAsync(Npc npc, CancellationToken ct = default)
    {
        npc.npc_id = _nextId++;
        _npcs.Add(npc);
        return Task.FromResult(npc.npc_id);
    }

    public Task<bool> UpdateNpcAsync(Npc npc, CancellationToken ct = default)
    {
        var existing = _npcs.FirstOrDefault(n => n.npc_id == npc.npc_id);
        if (existing == null) return Task.FromResult(false);
        
        existing.name = npc.name;
        existing.description = npc.description;
        existing.stats = npc.stats;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteNpcAsync(int npcId, CancellationToken ct = default)
    {
        var npc = _npcs.FirstOrDefault(n => n.npc_id == npcId);
        if (npc == null) return Task.FromResult(false);
        
        _npcs.Remove(npc);
        return Task.FromResult(true);
    }
}

internal class FakeGameSystemRepository : IGameSystemRepository
{
    private readonly List<Game_System> _gameSystems = new()
    {
        new Game_System { game_system_id = 1, game_system_name = "Dungeons & Dragons (5e)" },
        new Game_System { game_system_id = 2, game_system_name = "Pathfinder 2e" },
        new Game_System { game_system_id = 3, game_system_name = "Generic" }
    };

    public Task<IEnumerable<Game_System>> GetAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_gameSystems.AsEnumerable());
    }

    public Task<Game_System?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return Task.FromResult(_gameSystems.FirstOrDefault(gs => gs.game_system_id == id));
    }

    public Task<Game_System?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return Task.FromResult(_gameSystems.FirstOrDefault(gs => gs.game_system_name == name));
    }
}

internal class FakeCampaignRepository : ICampaignRepository
{
    private readonly List<Campaign> _campaigns = new()
    {
        new Campaign { campaign_id = 1, account_id = 10, game_system_id = 1, name = "Test Campaign 1" },
        new Campaign { campaign_id = 2, account_id = 10, game_system_id = 2, name = "Test Campaign 2" },
        new Campaign { campaign_id = 3, account_id = 5, game_system_id = 1, name = "Test Campaign 3" }
    };

    public Task<IEnumerable<Campaign>> GetByAccountIdAsync(int accountId, CancellationToken ct = default)
    {
        return Task.FromResult(_campaigns.Where(c => c.account_id == accountId).AsEnumerable());
    }

    public Task<Campaign?> GetByIdAsync(int campaignId, CancellationToken ct = default)
    {
        return Task.FromResult(_campaigns.FirstOrDefault(c => c.campaign_id == campaignId));
    }

    public Task<int> CreateAsync(Campaign campaign, CancellationToken ct = default)
    {
        campaign.campaign_id = _campaigns.Max(c => c.campaign_id) + 1;
        _campaigns.Add(campaign);
        return Task.FromResult(campaign.campaign_id);
    }

    public Task<bool> DeleteAsync(int campaignId, CancellationToken ct = default)
    {
        var campaign = _campaigns.FirstOrDefault(c => c.campaign_id == campaignId);
        if (campaign == null) return Task.FromResult(false);
        _campaigns.Remove(campaign);
        return Task.FromResult(true);
    }

    public Task<bool> UpdateAsync(Campaign campaign, CancellationToken ct = default)
    {
        var existing = _campaigns.FirstOrDefault(c => c.campaign_id == campaign.campaign_id);
        if (existing == null) return Task.FromResult(false);
        
        existing.name = campaign.name;
        existing.description = campaign.description;
        existing.game_system_id = campaign.game_system_id;
        return Task.FromResult(true);
    }
}

public class NpcLogicTests
{
    [Fact]
    public async Task GetNpcList_ReturnsMappedList()
    {
        // Arrange
        var npcs = new[]
        {
            new Npc { name="test", npc_id = 1, account_id = 10, campaign_id = 1, game_system_id = 1, stats = string.Empty},
            new Npc { name="test2",npc_id = 2, account_id = 10, campaign_id = 1, game_system_id = 1, stats = string.Empty }
        };
        var repo = new FakeNpcRepository(npcs);
        var campaignRepo = new FakeCampaignRepository();
        var logic = new NpcLogic(repo, campaignRepo, NullLogger<NpcLogic>.Instance);

        // Act
        var result = await logic.GetNpcList(10, null, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task GetNpcList_FiltersOnCampaignId_WhenProvided()
    {
        // Arrange
        var npcs = new[]
        {
            new Npc { name="Campaign1NPC", npc_id = 1, account_id = 10, campaign_id = 1, game_system_id = 1, stats = string.Empty},
            new Npc { name="Campaign2NPC", npc_id = 2, account_id = 10, campaign_id = 2, game_system_id = 1, stats = string.Empty },
            new Npc { name="Campaign1NPC2", npc_id = 3, account_id = 10, campaign_id = 1, game_system_id = 1, stats = string.Empty }
        };
        var repo = new FakeNpcRepository(npcs);
        var campaignRepo = new FakeCampaignRepository();
        var logic = new NpcLogic(repo, campaignRepo, NullLogger<NpcLogic>.Instance);

        // Act
        var result = await logic.GetNpcList(10, 1, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.All(list, npc => Assert.Equal(1, npc.Campaign_Id));
    }

    [Fact]
    public async Task GetNpc_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var repo = new FakeNpcRepository();
        var campaignRepo = new FakeCampaignRepository();
        var logic = new NpcLogic(repo, campaignRepo, NullLogger<NpcLogic>.Instance);

        // Act
        var result = await logic.GetNpc(999, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetNpc_ReturnsMappedNpc_WhenFound()
    {
        // Arrange
        var npc = new Npc { name = "SupGirl", npc_id = 42, account_id = 5, campaign_id = 3, game_system_id = 1, stats = string.Empty };
        var repo = new FakeNpcRepository(new[] { npc });
        var campaignRepo = new FakeCampaignRepository();
        var logic = new NpcLogic(repo, campaignRepo, NullLogger<NpcLogic>.Instance);

        // Act
        var result = await logic.GetNpc(42, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result!.Npc_Id);
    }

    [Fact]
    public async Task UpdateNpcAsync_ThrowsInvalidOperationException_WhenCampaignDoesNotExist()
    {
        // Arrange
        var npc = new Npc { name = "TestNpc", npc_id = 1, account_id = 10, campaign_id = 1, game_system_id = 1, stats = string.Empty };
        var repo = new FakeNpcRepository(new[] { npc });
        var campaignRepo = new FakeCampaignRepository();
        var logic = new NpcLogic(repo, campaignRepo, NullLogger<NpcLogic>.Instance);

        var updateRequest = new GM_Buddy.Contracts.Models.Npcs.UpdateNpcRequest
        {
            Name = "Updated NPC",
            CampaignId = 999, // Campaign that doesn't exist
            Description = "Test description",
            Race = "Elf",
            Class = "Wizard"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => logic.UpdateNpcAsync(1, 10, updateRequest, CancellationToken.None)
        );
        Assert.Contains("Campaign with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task UpdateNpcAsync_ThrowsUnauthorizedAccessException_WhenCampaignBelongsToDifferentAccount()
    {
        // Arrange
        var npc = new Npc { name = "TestNpc", npc_id = 1, account_id = 10, campaign_id = 1, game_system_id = 1, stats = string.Empty };
        var repo = new FakeNpcRepository(new[] { npc });
        var campaignRepo = new FakeCampaignRepository(); // Campaign 3 belongs to account 5
        var logic = new NpcLogic(repo, campaignRepo, NullLogger<NpcLogic>.Instance);

        var updateRequest = new GM_Buddy.Contracts.Models.Npcs.UpdateNpcRequest
        {
            Name = "Updated NPC",
            CampaignId = 3, // Campaign belongs to account 5, not 10
            Description = "Test description",
            Race = "Dwarf",
            Class = "Fighter"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => logic.UpdateNpcAsync(1, 10, updateRequest, CancellationToken.None)
        );
        Assert.Contains("Campaign 3 does not belong to account 10", exception.Message);
    }

    [Fact]
    public async Task UpdateNpcAsync_ReturnsTrue_WhenCampaignIsValidAndBelongsToAccount()
    {
        // Arrange
        var npc = new Npc { name = "TestNpc", npc_id = 1, account_id = 10, campaign_id = 1, game_system_id = 1, stats = string.Empty };
        var repo = new FakeNpcRepository(new[] { npc });
        var campaignRepo = new FakeCampaignRepository(); // Campaign 1 belongs to account 10
        var logic = new NpcLogic(repo, campaignRepo, NullLogger<NpcLogic>.Instance);

        var updateRequest = new GM_Buddy.Contracts.Models.Npcs.UpdateNpcRequest
        {
            Name = "Updated NPC",
            CampaignId = 1, // Valid campaign that belongs to account 10
            Description = "Test description",
            Race = "Human",
            Class = "Paladin"
        };

        // Act
        var result = await logic.UpdateNpcAsync(1, 10, updateRequest, CancellationToken.None);

        // Assert
        Assert.True(result);
    }
}