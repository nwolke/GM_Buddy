using GM_Buddy.Contracts.DTOs;

namespace GM_Buddy.Contracts.Interfaces;

public interface INpcLogic
{
    Task<IEnumerable<DndNpcDto>> GetNpcList(int user_id);
    Task<dynamic?> GetNpc(int npc_id);
    //Task<bool> AddNewNpc(NpcDto newNpc);
    //Task<bool> UpdateNpc(NpcDto updatedNpc);
    //Task<bool> DeleteNpc(int npc_id);
}
