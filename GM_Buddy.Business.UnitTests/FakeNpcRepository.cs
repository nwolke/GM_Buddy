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

    public Task<IEnumerable<Npc>> GetNpcsByAccountId(int accountId, CancellationToken ct = default)
    {
        var result = _npcs.Where(n => n.account_id == accountId);
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

public class NpcLogicTests
{
    [Fact]
    public async Task GetNpcList_ReturnsMappedList()
    {
        // Arrange
        var npcs = new[]
        {
            new Npc { name="test", npc_id = 1, account_id = 10, game_system_id = 1, stats = string.Empty},
            new Npc { name="test2",npc_id = 2, account_id = 10, game_system_id = 1, stats = string.Empty }
        };
        var repo = new FakeNpcRepository(npcs);
        var gameSystemRepo = new FakeGameSystemRepository();
        var logic = new NpcLogic(repo, gameSystemRepo, NullLogger<NpcLogic>.Instance);

        // Act
        var result = await logic.GetNpcList(10, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task GetNpc_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var repo = new FakeNpcRepository();
        var gameSystemRepo = new FakeGameSystemRepository();
        var logic = new NpcLogic(repo, gameSystemRepo, NullLogger<NpcLogic>.Instance);

        // Act
        var result = await logic.GetNpc(999, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetNpc_ReturnsMappedNpc_WhenFound()
    {
        // Arrange
        var npc = new Npc { name = "SupGirl", npc_id = 42, account_id = 5, game_system_id = 1, stats = string.Empty };
        var repo = new FakeNpcRepository(new[] { npc });
        var gameSystemRepo = new FakeGameSystemRepository();
        var logic = new NpcLogic(repo, gameSystemRepo, NullLogger<NpcLogic>.Instance);

        // Act
        var result = await logic.GetNpc(42, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result!.Npc_Id);
    }
}