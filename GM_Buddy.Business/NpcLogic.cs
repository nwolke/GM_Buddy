using GM_Buddy.Contracts.Interfaces;

namespace GM_Buddy.Business
{
    public class NpcLogic : INpcLogic
    {
        //public required GmBuddyDbContext _gmBuddyDbContext;
        //public NpcLogic(GmBuddyDbContext gmBuddyDbContext) {
        //_gmBuddyDbContext = gmBuddyDbContext;
        //}

        //public IEnumerable<NpcDto> GetNpcList(int account_id)
        //{
        //    var allNpcs = from npc in _gmBuddyDbContext.Npcs
        //                  where npc.account.account_id == account_id
        //                  select new NpcDto
        //                  {
        //                      npc_id = npc.npc_id,
        //                      account = npc.account.account_name,
        //                      name = npc.name,
        //                      stats = JsonSerializer.Deserialize<DnDStats>(npc.stats ?? "{}", JsonSerializerOptions.Default),
        //                      description = npc.description,
        //                      lineage = npc.lineage.lineage_name,
        //                      occupation = npc.occupation.occupation_name,
        //                      system = npc.game_system.game_system_name
        //                  };
        //    return allNpcs;
        //}

        //public async Task<NpcDto?> GetNpc(int npc_id)
        //{
        //    var singleNpc = await (from npc in _gmBuddyDbContext.Npcs
        //                     where npc.npc_id == npc_id
        //                     select new NpcDto
        //                     {
        //                         npc_id = npc.npc_id,
        //                         account = npc.account.account_name,
        //                         name = npc.name,
        //                         stats = JsonSerializer.Deserialize<DnDStats>(npc.stats ?? "{}", JsonSerializerOptions.Default),
        //                         description = npc.description,
        //                         lineage = npc.lineage.lineage_name,
        //                         occupation = npc.occupation.occupation_name,
        //                         system = npc.game_system.game_system_name
        //                     }).FirstOrDefaultAsync();
        //    return singleNpc;
        //}

        //public async Task<bool> AddNewNpc(NpcDto newNpc)
        //{
        //    _gmBuddyDbContext.Npcs.Add(new Npc
        //    {
        //        account = _gmBuddyDbContext.Accounts.Where(x => x.account_name == newNpc.account).First(),
        //        name = newNpc.name,
        //        description = newNpc.description,
        //        stats = JsonSerializer.Serialize(newNpc.stats),
        //        lineage = _gmBuddyDbContext.Lineages.Where(x => x.lineage_name.Equals(newNpc.lineage)).First(),
        //        occupation = _gmBuddyDbContext.Occupations.Where(x => x.occupation_name.Equals(newNpc.occupation)).First(),
        //        game_system = _gmBuddyDbContext.GameSystems.Where(x => x.game_system_name.Equals(newNpc.system)).First(),
        //    });

        //    var result = await _gmBuddyDbContext.SaveChangesAsync();
        //    return result > 0;
        //}

        //public async Task<bool> UpdateNpc(NpcDto updatedNpc)
        //{
        //    var npcToUpdate = _gmBuddyDbContext.Npcs.FirstOrDefault(x => x.npc_id == updatedNpc.npc_id);
        //    if (npcToUpdate != null)
        //    {
        //        npcToUpdate.description = updatedNpc.description;
        //        npcToUpdate.name = updatedNpc.name;
        //        npcToUpdate.stats = JsonSerializer.Serialize(updatedNpc.stats);
        //        npcToUpdate.lineage = _gmBuddyDbContext.Lineages.First(x => x.lineage_name == updatedNpc.lineage);
        //        npcToUpdate.occupation = _gmBuddyDbContext.Occupations.First(x => x.occupation_name == updatedNpc.occupation);

        //        var result = await _gmBuddyDbContext.SaveChangesAsync();
        //        return result > 0;
        //    }
        //    return false;
        //}

        //public async Task<bool> DeleteNpc(int npc_id)
        //{
        //    var toRemove = await _gmBuddyDbContext.Npcs.Where(x => x.npc_id == npc_id).FirstOrDefaultAsync();
        //    if (toRemove != null)
        //    {
        //        _gmBuddyDbContext.Npcs.Remove(toRemove);
        //        await _gmBuddyDbContext.SaveChangesAsync();
        //        return true;
        //    }    
        //    return false;
        //}
    }
}
