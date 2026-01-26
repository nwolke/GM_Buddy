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
    private readonly IGameSystemRepository _gameSystemRepository;
    private readonly ILogger<NpcLogic> _logger;

    // Default game system name for D&D 5e
    private const string DefaultGameSystemName = "Dungeons & Dragons (5e)";

    public NpcLogic(
        INpcRepository npcRepository, 
        IGameSystemRepository gameSystemRepository,
        ILogger<NpcLogic> logger)
    {
        _npcRepository = npcRepository;
        _gameSystemRepository = gameSystemRepository;
        _logger = logger;
    }

    /// <summary>
    /// Resolves a game system ID from the request system name, falling back to the default system.
    /// </summary>
    /// <param name="requestedSystemName">The system name from the request (may be null or empty)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The resolved game system ID</returns>
    /// <exception cref="InvalidOperationException">Thrown when the default game system is not found in the database</exception>
    private async Task<int> ResolveGameSystemIdAsync(string? requestedSystemName, CancellationToken ct = default)
    {
        // Try to use the requested system if provided
        if (!string.IsNullOrWhiteSpace(requestedSystemName))
        {
            var gameSystem = await _gameSystemRepository.GetByNameAsync(requestedSystemName, ct);
            if (gameSystem != null)
            {
                _logger.LogInformation("Using game system: {SystemName} (ID: {SystemId})", 
                    requestedSystemName, gameSystem.game_system_id);
                return gameSystem.game_system_id;
            }
            
            _logger.LogWarning("Game system '{SystemName}' not found, using default", requestedSystemName);
        }

        // Fall back to default system
        var defaultSystem = await _gameSystemRepository.GetByNameAsync(DefaultGameSystemName, ct);
        if (defaultSystem == null)
        {
            var errorMsg = $"Default game system '{DefaultGameSystemName}' not found in database. Please ensure init.sql has been run.";
            _logger.LogError(errorMsg);
            throw new InvalidOperationException(errorMsg);
        }

        _logger.LogInformation("Using default game system: {SystemName} (ID: {SystemId})", 
            DefaultGameSystemName, defaultSystem.game_system_id);
        return defaultSystem.game_system_id;
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
            // Resolve game system ID from request or use default
            int gameSystemId = await ResolveGameSystemIdAsync(request.System, ct);

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
                game_system_id = gameSystemId,
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
            // Resolve game system ID from request or use default
            int gameSystemId = await ResolveGameSystemIdAsync(request.System, ct);

            // Build a simple stats JSON from the request
            var stats = new
            {
                lineage = request.Race ?? "Unknown",
                @class = request.Class ?? "Adventurer",
                faction = request.Faction,
                notes = request.Notes
            };

            var npc = new Npc
            {
                npc_id = npcId,
                account_id = accountId,
                game_system_id = gameSystemId,
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