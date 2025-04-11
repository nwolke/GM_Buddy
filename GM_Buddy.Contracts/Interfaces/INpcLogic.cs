using GM_Buddy.Contracts.DbModels;

namespace GM_Buddy.Contracts.Interfaces;

public interface INpcLogic
{
    Task<IEnumerable<npc_type>> GetNpcList(int account_id);
    Task<dynamic?> GetNpc(int npc_id);
    //Task<bool> AddNewNpc(NpcDto newNpc);
    //Task<bool> UpdateNpc(NpcDto updatedNpc);
    //Task<bool> DeleteNpc(int npc_id);
}
