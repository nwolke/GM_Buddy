using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Contracts.Models.Npcs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace GM_Buddy.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class NpcsController : ControllerBase
{
    private readonly ILogger<NpcsController> _logger;
    private readonly INpcLogic _logic;
    private readonly IAccountRepository _accountRepository;

    public NpcsController(
        ILogger<NpcsController> logger, 
        INpcLogic npcLogic,
        IAccountRepository accountRepository)
    {
        _logger = logger;
        _logic = npcLogic;
        _accountRepository = accountRepository;
    }

    /// <summary>
    /// Get all NPCs for an account, optionally filtered by game system.
    /// Requires cognitoSub to identify the user. Account must exist (call /account/sync first).
    /// </summary>
    [HttpGet]
    [OutputCache(PolicyName = "NpcList")]
    public async Task<ActionResult<IEnumerable<BaseNpc>>> GetNpcs(
        [FromQuery] string cognitoSub,
        [FromQuery] string? gameSystem = null)
    {
        if (string.IsNullOrEmpty(cognitoSub))
        {
            return BadRequest("cognitoSub is required");
        }

        try
        {
            // Look up account by Cognito sub
            var account = await _accountRepository.GetByCognitoSubAsync(cognitoSub);
            if (account == null)
            {
                _logger.LogWarning("Account not found for cognitoSub: {CognitoSub}", cognitoSub);
                return NotFound("Account not found. Please sync account first.");
            }
            
            _logger.LogInformation("Getting NPCs for account {AccountId}", account.account_id);
            IEnumerable<BaseNpc> result = await _logic.GetNpcList(account.account_id);
            
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
            _logger.LogError(ex, "Error retrieving NPCs for {CognitoSub}", cognitoSub);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific NPC by ID
    /// </summary>
    [HttpGet("{id}")]
    [OutputCache(PolicyName = "ShortCache")]
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
    [OutputCache(PolicyName = "NpcList")]
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
    [OutputCache(PolicyName = "NpcList")]
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
    [OutputCache(PolicyName = "ShortCache")]
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

    /// <summary>
    /// Create a new NPC
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BaseNpc>> CreateNpc([FromBody] CreateNpcRequest request, [FromQuery] int accountId)
    {
        if (accountId <= 0)
        {
            return BadRequest("Valid accountId is required");
        }

        try
        {
            int npcId = await _logic.CreateNpcAsync(accountId, request);
            _logger.LogInformation("Created NPC {NpcId} for account {AccountId}", npcId, accountId);
            
            // Fetch the created NPC to return
            var createdNpc = await _logic.GetNpc(npcId);
            if (createdNpc == null)
            {
                return StatusCode(500, "NPC was created but could not be retrieved");
            }
            
            return CreatedAtAction(nameof(GetNpc), new { id = npcId }, createdNpc);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating NPC for account {AccountId}", accountId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing NPC
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateNpc(int id, [FromBody] UpdateNpcRequest request, [FromQuery] int accountId)
    {
        if (accountId <= 0)
        {
            return BadRequest("Valid accountId is required");
        }

        try
        {
            bool success = await _logic.UpdateNpcAsync(id, accountId, request);
            if (!success)
            {
                return NotFound($"NPC with ID {id} not found or not owned by account {accountId}");
            }
            
            _logger.LogInformation("Updated NPC {NpcId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating NPC {NpcId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete an NPC
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteNpc(int id)
    {
        try
        {
            bool success = await _logic.DeleteNpcAsync(id);
            if (!success)
            {
                return NotFound($"NPC with ID {id} not found");
            }
            
            _logger.LogInformation("Deleted NPC {NpcId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting NPC {NpcId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
