using GM_Buddy.Contracts;
using GM_Buddy.Contracts.DbModels;
using GM_Buddy.Contracts.DTOs;
using System.Text.Json;

namespace GM_Buddy.Business.Mappers;

public class NpcMapper
{
    public static BaseNpc MapToNpcDto(npc_type npc)
    {
        return new BaseNpc
        {
            Npc_Id = npc.npc_id,
            UserId = npc.user_id,
            Name = npc.name,
            Lineage = npc.lineage_name,
            Occupation = npc.occupation_name,
            System = npc.game_system_name,
            Stats = JsonSerializer.Deserialize<DnDStats>(npc.stats ?? "{}", JsonSerializerOptions.Default),
            Description = npc.description
        };
    }
}
