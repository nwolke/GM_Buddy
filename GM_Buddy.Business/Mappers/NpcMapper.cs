using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Npcs.Dnd;
using System.Text.Json;

namespace GM_Buddy.Business.Mappers;

public class NpcMapper
{
    public static DndNpc MapToNpcDto(Npc npc)
    {
        // Safe fallback for stats: build a minimal DnDStats if deserialization fails or stats is null/empty.
        DnDStats stats;
        try
        {
            stats = JsonSerializer.Deserialize<DnDStats>(
                npc.stats ?? "{}",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new DnDStats { Attributes = new DndAttributes(), Languages = Array.Empty<string>() };
        }
        catch
        {
            stats = new DnDStats { Attributes = new DndAttributes(), Languages = Array.Empty<string>() };
        }

        return new DndNpc
        {
            Npc_Id = npc.npc_id,
            User_Id = npc.user_id,
            Name = npc.name,
            Lineage = npc.lineage_name,
            Occupation = npc.occupation_name,
            System = npc.game_system_name,
            Stats = stats,
            Description = npc.description,
            Gender = npc.gender
        };
    }
}
