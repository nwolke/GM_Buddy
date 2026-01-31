using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;

namespace GM_Buddy.Business.ComponentTests.Fakes;

internal class FakeNpcRepository : INpcRepository
{
    private readonly Dictionary<int, Npc> _npcs = new();
    private int _nextId = 1;

    public void AddNpc(Npc npc)
    {
        if (npc.npc_id == 0)
        {
            npc.npc_id = _nextId++;
        }
        _npcs[npc.npc_id] = npc;
    }

    public Task<IEnumerable<Npc>> GetNpcs(int account_id, int? campaignId = null, CancellationToken ct = default)
    {
        var npcs = _npcs.Values.Where(n => n.account_id == account_id);
        if (campaignId.HasValue)
        {
            npcs = npcs.Where(n => n.campaign_id == campaignId.Value);
        }
        return Task.FromResult(npcs);
    }

    public Task<Npc?> GetNpcById(int npcId, CancellationToken ct = default)
    {
        _npcs.TryGetValue(npcId, out var npc);
        return Task.FromResult(npc);
    }

    public Task<int> CreateNpcAsync(Npc npc, CancellationToken ct = default)
    {
        npc.npc_id = _nextId++;
        _npcs[npc.npc_id] = npc;
        return Task.FromResult(npc.npc_id);
    }

    public Task<bool> UpdateNpcAsync(Npc npc, CancellationToken ct = default)
    {
        if (!_npcs.ContainsKey(npc.npc_id))
        {
            return Task.FromResult(false);
        }
        _npcs[npc.npc_id] = npc;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteNpcAsync(int npcId, CancellationToken ct = default)
    {
        return Task.FromResult(_npcs.Remove(npcId));
    }
}
