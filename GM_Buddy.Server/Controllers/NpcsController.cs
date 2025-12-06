using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Contracts.Models.Npcs;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class NpcsController : ControllerBase
{
    private readonly ILogger<NpcsController> _logger;
    private readonly INpcLogic _logic;

    public NpcsController(ILogger<NpcsController> logger, INpcLogic npcLogic)
    {
        _logger = logger;
        _logic = npcLogic;
    }

    /// <summary>
    /// Get all NPCs for an account, optionally filtered by game system
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BaseNpc>>> GetNpcs(
        [FromQuery] int accountId,
        [FromQuery] string? gameSystem = null)
    {
        try
        {
            _logger.LogInformation("Getting NPCs for account {AccountId}", accountId);
            IEnumerable<BaseNpc> result = await _logic.GetNpcList(accountId);
            
            if (!string.IsNullOrWhiteSpace(gameSystem))
            {
                result = result.Where(n => n.System != null && 
                    n.System.Equals(gameSystem, StringComparison.OrdinalIgnoreCase));
            }
            
            _logger.LogInformation("Retrieved {Count} NPCs", result.Count());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NPCs for account {AccountId}", accountId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific NPC by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BaseNpc>> GetNpc(int id)
    {
        try
        {
            BaseNpc? npc = await _logic.GetNpc(id);
            if (npc == null)
            {
                return NotFound($"NPC with ID {id} not found");
            }
            return Ok(npc);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NPC {NpcId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all NPCs for a specific account
    /// </summary>
    [HttpGet("account/{accountId}")]
    public async Task<ActionResult<IEnumerable<BaseNpc>>> GetNpcsByAccount(int accountId)
    {
        try
        {
            _logger.LogInformation("Getting NPCs for account {AccountId}", accountId);
            IEnumerable<BaseNpc> result = await _logic.GetNpcList(accountId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NPCs for account {AccountId}", accountId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all NPCs for a specific game system
    /// </summary>
    [HttpGet("game-system/{gameSystem}")]
    public async Task<ActionResult<IEnumerable<BaseNpc>>> GetNpcsByGameSystem(
        string gameSystem,
        [FromQuery] int? accountId = null)
    {
        try
        {
            if (!accountId.HasValue)
            {
                return BadRequest("Account ID is required");
            }

            IEnumerable<BaseNpc> npcs = await _logic.GetNpcList(accountId.Value);
            IEnumerable<BaseNpc> filtered = npcs.Where(n => n.System != null && 
                n.System.Equals(gameSystem, StringComparison.OrdinalIgnoreCase));
            
            _logger.LogInformation("Retrieved {Count} NPCs for game system {GameSystem}", 
                filtered.Count(), gameSystem);
            
            return Ok(filtered);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NPCs for game system {GameSystem}", gameSystem);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search NPCs by name or other criteria
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<BaseNpc>>> SearchNpcs(
        [FromQuery] int accountId,
        [FromQuery] string? name = null,
        [FromQuery] string? lineage = null,
        [FromQuery] string? occupation = null)
    {
        try
        {
            IEnumerable<BaseNpc> npcs = await _logic.GetNpcList(accountId);
            
            if (!string.IsNullOrWhiteSpace(name))
            {
                npcs = npcs.Where(n => n.Name != null && 
                    n.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            // Note: lineage and occupation filtering would require accessing the Stats object
            // which varies by game system. This is a simplified example.
            
            _logger.LogInformation("Search returned {Count} NPCs", npcs.Count());
            return Ok(npcs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching NPCs");
            return StatusCode(500, "Internal server error");
        }
    }

    // TODO: Implement POST, PUT, DELETE endpoints when NpcLogic supports them
    // TODO: Implement relationship shortcuts
    // TODO: Implement export/import functionality
}
