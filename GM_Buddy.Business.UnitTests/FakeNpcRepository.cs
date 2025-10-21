using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GM_Buddy.Business;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Contracts.Npcs.Dnd;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GM_Buddy.Business.UnitTests;

internal class FakeNpcRepository : INpcRepository
{
    private readonly List<Npc> _npcs;

    public FakeNpcRepository(IEnumerable<Npc>? npcs = null)
    {
        _npcs = npcs?.ToList() ?? new List<Npc>();
    }

    public Task<IEnumerable<Npc>> GetNpcsByAccountId(int accountId, CancellationToken ct = default)
    {
        var result = _npcs.Where(n => n.user_id == accountId);
        return Task.FromResult(result.AsEnumerable());
    }

    public Task<Npc?> GetNpcById(int npcId, CancellationToken ct = default)
    {
        var npc = _npcs.FirstOrDefault(n => n.npc_id == npcId);
        return Task.FromResult(npc);
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
            new Npc { npc_id = 1, user_id = 10, name = "A", stats = "{}", description = "d", gender = "m", game_system_id = 1, lineage_id =1, occupation_id=1 },
            new Npc { npc_id = 2, user_id = 10, name = "B", stats = "{}", description = "d2", gender = "f", game_system_id = 1, lineage_id =1, occupation_id=1 }
        };
        var repo = new FakeNpcRepository(npcs);
        var logic = new NpcLogic(repo, NullLogger<NpcLogic>.Instance);

        // Act
        var result = await logic.GetNpcList(10, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, x => x.Name == "A");
        Assert.Contains(list, x => x.Name == "B");
    }

    [Fact]
    public async Task GetNpc_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var repo = new FakeNpcRepository();
        var logic = new NpcLogic(repo, NullLogger<NpcLogic>.Instance);

        // Act
        var result = await logic.GetNpc(999, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetNpc_ReturnsMappedNpc_WhenFound()
    {
        // Arrange
        var npc = new Npc { npc_id = 42, user_id = 5, name = "Found", stats = "{}", description = "ok", gender = "m", game_system_id = 1, lineage_id =1, occupation_id=1 };
        var repo = new FakeNpcRepository(new[] { npc });
        var logic = new NpcLogic(repo, NullLogger<NpcLogic>.Instance);

        // Act
        var result = await logic.GetNpc(42, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result!.Npc_Id);
        Assert.Equal("Found", result.Name);
    }
}