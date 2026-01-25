using GM_Buddy.Contracts.DbEntities;

namespace GM_Buddy.Contracts.Interfaces;

public interface INpcRepository
{
    Task<IEnumerable<Npc>> GetNpcsByAccountId(int accountId, CancellationToken ct = default);
    Task<Npc?> GetNpcById(int npcId, CancellationToken ct = default);
    Task<int> CreateNpcAsync(Npc npc, CancellationToken ct = default);
    Task<bool> UpdateNpcAsync(Npc npc, CancellationToken ct = default);
    Task<bool> DeleteNpcAsync(int npcId, CancellationToken ct = default);
}