using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Contracts.Models.Npcs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Security.Claims;

namespace GM_Buddy.Server.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
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
    /// Helper method to get the authenticated user's account ID from JWT claims
    /// </summary>
    private async Task<(int accountId, string error)?> GetAuthenticatedAccountIdAsync()
    {
        var cognitoSub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(cognitoSub))
        {
            _logger.LogWarning("No user identifier found in claims");
            return (0, "User authentication failed");
        }

        var account = await _accountRepository.GetByCognitoSubAsync(cognitoSub);
        if (account == null)
        {
            _logger.LogWarning("Account not found for cognitoSub: {CognitoSub}", cognitoSub);
            return (0, "Account not found. Please sync account first.");
        }

        return (account.account_id, string.Empty);
    }

    /// <summary>
    /// Get all NPCs for the authenticated user's account, optionally filtered by game system.
    /// Account must exist (call /account/sync first).
    /// </summary>
    [HttpGet]
    [OutputCache(PolicyName = "NpcList")]
    public async Task<ActionResult<IEnumerable<BaseNpc>>> GetNpcs(
        [FromQuery] string? gameSystem = null)
    {
        try
        {
            var accountResult = await GetAuthenticatedAccountIdAsync();
            if (accountResult.HasValue && !string.IsNullOrEmpty(accountResult.Value.error))
            {
                return accountResult.Value.accountId == 0 
                    ? NotFound(accountResult.Value.error) 
                    : BadRequest(accountResult.Value.error);
            }

            int accountId = accountResult!.Value.accountId;
            
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
            _logger.LogError(ex, "Error retrieving NPCs");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific NPC by ID (must be owned by the authenticated user)
    /// </summary>
    [HttpGet("{id}")]
    [OutputCache(PolicyName = "ShortCache")]
    public async Task<ActionResult<BaseNpc>> GetNpc(int id)
    {
        try
        {
            var accountResult = await GetAuthenticatedAccountIdAsync();
            if (accountResult.HasValue && !string.IsNullOrEmpty(accountResult.Value.error))
            {
                return accountResult.Value.accountId == 0 
                    ? NotFound(accountResult.Value.error) 
                    : BadRequest(accountResult.Value.error);
            }

            int accountId = accountResult!.Value.accountId;

            BaseNpc? npc = await _logic.GetNpc(id);
            if (npc == null)
            {
                return NotFound($"NPC with ID {id} not found");
            }

            // Verify the NPC belongs to the authenticated user's account
            if (npc.Account_Id != accountId)
            {
                _logger.LogWarning("User attempted to access NPC {NpcId} not owned by their account {AccountId}", id, accountId);
                return Forbid();
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
    /// Get all NPCs for the authenticated user's account
    /// </summary>
    [HttpGet("account")]
    [OutputCache(PolicyName = "NpcList")]
    public async Task<ActionResult<IEnumerable<BaseNpc>>> GetNpcsByAccount()
    {
        try
        {
            var accountResult = await GetAuthenticatedAccountIdAsync();
            if (accountResult.HasValue && !string.IsNullOrEmpty(accountResult.Value.error))
            {
                return accountResult.Value.accountId == 0 
                    ? NotFound(accountResult.Value.error) 
                    : BadRequest(accountResult.Value.error);
            }

            int accountId = accountResult!.Value.accountId;

            _logger.LogInformation("Getting NPCs for account {AccountId}", accountId);
            IEnumerable<BaseNpc> result = await _logic.GetNpcList(accountId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NPCs");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all NPCs for a specific game system from the authenticated user's account
    /// </summary>
    [HttpGet("game-system/{gameSystem}")]
    [OutputCache(PolicyName = "NpcList")]
    public async Task<ActionResult<IEnumerable<BaseNpc>>> GetNpcsByGameSystem(string gameSystem)
    {
        try
        {
            var accountResult = await GetAuthenticatedAccountIdAsync();
            if (accountResult.HasValue && !string.IsNullOrEmpty(accountResult.Value.error))
            {
                return accountResult.Value.accountId == 0 
                    ? NotFound(accountResult.Value.error) 
                    : BadRequest(accountResult.Value.error);
            }

            int accountId = accountResult!.Value.accountId;

            IEnumerable<BaseNpc> npcs = await _logic.GetNpcList(accountId);
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
    /// Search NPCs by name or other criteria in the authenticated user's account
    /// </summary>
    [HttpGet("search")]
    [OutputCache(PolicyName = "ShortCache")]
    public async Task<ActionResult<IEnumerable<BaseNpc>>> SearchNpcs(
        [FromQuery] string? name = null,
        [FromQuery] string? lineage = null,
        [FromQuery] string? occupation = null)
    {
        try
        {
            var accountResult = await GetAuthenticatedAccountIdAsync();
            if (accountResult.HasValue && !string.IsNullOrEmpty(accountResult.Value.error))
            {
                return accountResult.Value.accountId == 0 
                    ? NotFound(accountResult.Value.error) 
                    : BadRequest(accountResult.Value.error);
            }

            int accountId = accountResult!.Value.accountId;

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
    /// Create a new NPC for the authenticated user's account
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BaseNpc>> CreateNpc([FromBody] CreateNpcRequest request)
    {
        try
        {
            var accountResult = await GetAuthenticatedAccountIdAsync();
            if (accountResult.HasValue && !string.IsNullOrEmpty(accountResult.Value.error))
            {
                return accountResult.Value.accountId == 0 
                    ? NotFound(accountResult.Value.error) 
                    : BadRequest(accountResult.Value.error);
            }

            int accountId = accountResult!.Value.accountId;

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
            _logger.LogError(ex, "Error creating NPC");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing NPC owned by the authenticated user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateNpc(int id, [FromBody] UpdateNpcRequest request)
    {
        try
        {
            var accountResult = await GetAuthenticatedAccountIdAsync();
            if (accountResult.HasValue && !string.IsNullOrEmpty(accountResult.Value.error))
            {
                return accountResult.Value.accountId == 0 
                    ? NotFound(accountResult.Value.error) 
                    : BadRequest(accountResult.Value.error);
            }

            int accountId = accountResult!.Value.accountId;

            bool success = await _logic.UpdateNpcAsync(id, accountId, request);
            if (!success)
            {
                return NotFound($"NPC with ID {id} not found or not owned by your account");
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
    /// Delete an NPC owned by the authenticated user
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteNpc(int id)
    {
        try
        {
            var accountResult = await GetAuthenticatedAccountIdAsync();
            if (accountResult.HasValue && !string.IsNullOrEmpty(accountResult.Value.error))
            {
                return accountResult.Value.accountId == 0 
                    ? NotFound(accountResult.Value.error) 
                    : BadRequest(accountResult.Value.error);
            }

            int accountId = accountResult!.Value.accountId;

            // First verify the NPC exists and belongs to the authenticated user
            var npc = await _logic.GetNpc(id);
            if (npc == null)
            {
                return NotFound($"NPC with ID {id} not found");
            }

            if (npc.Account_Id != accountId)
            {
                _logger.LogWarning("User attempted to delete NPC {NpcId} not owned by their account {AccountId}", id, accountId);
                return Forbid();
            }

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
