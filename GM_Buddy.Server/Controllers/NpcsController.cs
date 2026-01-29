using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Contracts.Models.Npcs;
using GM_Buddy.Server.Helpers;
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
    private readonly IAuthHelper _authHelper;

    public NpcsController(
        ILogger<NpcsController> logger,
        INpcLogic npcLogic,
        IAccountRepository accountRepository,
        IAuthHelper authHelper)
    {
        _logger = logger;
        _logic = npcLogic;
        _accountRepository = accountRepository;
        _authHelper = authHelper;
    }

    /// <summary>
    /// Get all NPCs for the authenticated user's account, optionally filtered by campaign.
    /// Account must exist (call /account/sync first).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BaseNpc>>> GetNpcs(
        [FromQuery] int? campaignId = null)
    {
        try
        {
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

            _logger.LogInformation("Getting NPCs for account {AccountId}", accountId);
            IEnumerable<BaseNpc> result = await _logic.GetNpcList(accountId, campaignId);
            _logger.LogInformation("Retrieved {Count} NPCs", result.Count());
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
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
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

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
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving NPC {NpcId}", id);
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
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

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
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
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
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

            bool success = await _logic.UpdateNpcAsync(id, accountId, request);
            if (!success)
            {
                return NotFound($"NPC with ID {id} not found or not owned by your account");
            }

            _logger.LogInformation("Updated NPC {NpcId}", id);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
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
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

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
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting NPC {NpcId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
