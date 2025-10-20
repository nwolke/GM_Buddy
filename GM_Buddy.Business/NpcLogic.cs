using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Contracts.Npcs.Dnd;
using Microsoft.Extensions.Logging;

namespace GM_Buddy.Business;

public class NpcLogic : INpcLogic
{
    private readonly INpcRepository _npcRepository;
    private readonly ILogger<NpcLogic> _logger;

    public NpcLogic(INpcRepository npcRepository, ILogger<NpcLogic> logger)
    {
        _npcRepository = npcRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<DndNpc>> GetNpcList(int user_id, CancellationToken ct = default)
    {
        var allNpcs = await _npcRepository.GetNpcsByAccountId(user_id, ct);
        return allNpcs?.Select(Mappers.NpcMapper.MapToNpcDto) ?? [];
    }

    public async Task<DndNpc?> GetNpc(int npc_id, CancellationToken ct = default)
    {
        try
        {
            var npc = await _npcRepository.GetNpcById(npc_id, ct);
            if (npc is null) return null;
            return Mappers.NpcMapper.MapToNpcDto(npc);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching NPC {NpcId}", npc_id);
            return null;
        }
    }
}