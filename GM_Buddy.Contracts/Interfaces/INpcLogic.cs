using GM_Buddy.Contracts.Models.Npcs;
using GM_Buddy.Contracts.Models.Npcs.Dnd;

namespace GM_Buddy.Contracts.Interfaces;

public interface INpcLogic
{
    Task<IEnumerable<DndNpc>> GetNpcList(int account_id, int? campaignId, CancellationToken ct = default);
    Task<DndNpc?> GetNpc(int npc_id, CancellationToken ct = default);
    Task<int> CreateNpcAsync(int accountId, CreateNpcRequest request, CancellationToken ct = default);
    Task<bool> UpdateNpcAsync(int npcId, int accountId, UpdateNpcRequest request, CancellationToken ct = default);
    Task<bool> DeleteNpcAsync(int npcId, CancellationToken ct = default);
}