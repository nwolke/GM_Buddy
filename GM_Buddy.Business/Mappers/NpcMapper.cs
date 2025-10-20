using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Npcs.Dnd;
using System.Text.Json;

namespace GM_Buddy.Business.Mappers;

public class NpcMapper
{
    public static DndNpc MapToNpcDto(Npc npc)
    {
        return new DndNpc
        {
            Npc_Id = npc.npc_id,
            User_Id = npc.user_id,
            Name = npc.name,
            Lineage = npc.lineage_name,
            Occupation = npc.occupation_name,
            System = npc.game_system_name,
            Stats = JsonSerializer.Deserialize<DnDStats>(npc.stats ?? "{}", JsonSerializerOptions.Default),
            Description = npc.description
        };
    }
}
