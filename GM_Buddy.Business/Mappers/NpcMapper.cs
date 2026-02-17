using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Models.Npcs.Dnd;
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
            ) ?? new DnDStats();
        }
        catch (Exception)
        {
            stats = new DnDStats();
        }

        return new DndNpc
        {
            Npc_Id = npc.npc_id,
            Account_Id = npc.account_id,
            Campaign_Id = npc.campaign_id,
            Name = npc.name,
            Description = npc.description,
            Stats = stats
        };
    }
}
