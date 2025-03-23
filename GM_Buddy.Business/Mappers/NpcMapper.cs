using GM_Buddy.Contracts;
using GM_Buddy.Contracts.DbModels;
using GM_Buddy.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GM_Buddy.Business.Mappers;

public class NpcMapper
{
    public static NpcDto MapToNpcDto(npc_type npc)
    {
        return new NpcDto
        {
            Npc_Id = npc.npc_id,
            Account = npc.account_name,
            Name = npc.name,
            Lineage = npc.lineage_name,
            Occupation = npc.occupation_name,
            System = npc.game_system_name,
            Stats = JsonSerializer.Deserialize<DnDStats>(npc.stats ?? "{}", JsonSerializerOptions.Default),
            Description = npc.description
        };
    }
}
