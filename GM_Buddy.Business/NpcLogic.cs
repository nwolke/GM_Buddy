using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Contracts.Models.Npcs;
using GM_Buddy.Contracts.Models.Npcs.Dnd;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GM_Buddy.Business;

public class NpcLogic : INpcLogic
{
    private readonly INpcRepository _npcRepository;
    private readonly ILogger<NpcLogic> _logger;

    // Default game system ID for D&D 5e (assuming it exists in the database)
    private const int DefaultGameSystemId = 1;

    public NpcLogic(INpcRepository npcRepository, ILogger<NpcLogic> logger)
    {
        _npcRepository = npcRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<DndNpc>> GetNpcList(int account_id, CancellationToken ct = default)
    {
        var allNpcs = await _npcRepository.GetNpcsByAccountId(account_id, ct);
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

    public async Task<int> CreateNpcAsync(int accountId, CreateNpcRequest request, CancellationToken ct = default)
    {
        try
        {
            // Build a simple stats JSON from the request
            var stats = new
            {
                race = request.Race ?? "Unknown",
                @class = request.Class ?? "Adventurer",
                faction = request.Faction,
                notes = request.Notes
            };

            var npc = new Npc
            {
                account_id = accountId,
                game_system_id = DefaultGameSystemId,
                name = request.Name,
                description = request.Description,
                stats = JsonSerializer.Serialize(stats)
            };

            int npcId = await _npcRepository.CreateNpcAsync(npc, ct);
            _logger.LogInformation("Created NPC {NpcId} for account {AccountId}", npcId, accountId);
            return npcId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating NPC for account {AccountId}", accountId);
            throw;
        }
    }

    public async Task<bool> UpdateNpcAsync(int npcId, int accountId, UpdateNpcRequest request, CancellationToken ct = default)
    {
        try
        {
            // Build a simple stats JSON from the request
            var stats = new
            {
                race = request.Race ?? "Unknown",
                @class = request.Class ?? "Adventurer",
                faction = request.Faction,
                notes = request.Notes
            };

            var npc = new Npc
            {
                npc_id = npcId,
                account_id = accountId,
                game_system_id = DefaultGameSystemId,
                name = request.Name,
                description = request.Description,
                stats = JsonSerializer.Serialize(stats)
            };

            bool success = await _npcRepository.UpdateNpcAsync(npc, ct);
            if (success)
            {
                _logger.LogInformation("Updated NPC {NpcId}", npcId);
            }
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating NPC {NpcId}", npcId);
            throw;
        }
    }

    public async Task<bool> DeleteNpcAsync(int npcId, CancellationToken ct = default)
    {
        try
        {
            bool success = await _npcRepository.DeleteNpcAsync(npcId, ct);
            if (success)
            {
                _logger.LogInformation("Deleted NPC {NpcId}", npcId);
            }
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting NPC {NpcId}", npcId);
            throw;
        }
    }
}