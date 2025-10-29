using GM_Buddy.Contracts.Npcs.Dnd;

namespace GM_Buddy.Contracts.Interfaces;

public interface INpcLogic
{
    Task<IEnumerable<DndNpc>> GetNpcList(int account_id, CancellationToken ct = default);
    Task<DndNpc?> GetNpc(int npc_id, CancellationToken ct = default);
    //Task<bool> AddNewNpc(NpcDto newNpc);
    //Task<bool> UpdateNpc(NpcDto updatedNpc);
    //Task<bool> DeleteNpc(int npc_id);
}