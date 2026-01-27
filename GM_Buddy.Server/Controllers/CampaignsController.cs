using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Server.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class CampaignsController : ControllerBase
{
    private readonly ILogger<CampaignsController> _logger;
    private readonly ICampaignLogic _campaignLogic;
    private readonly IAuthHelper _authHelper;

    public CampaignsController(
        ILogger<CampaignsController> logger,
        ICampaignLogic campaignLogic,
        IAuthHelper authHelper)
    {
        _logger = logger;
        _campaignLogic = campaignLogic;
        _authHelper = authHelper;
    }

    /// <summary>
    /// Get all campaigns for the authenticated user's account
    /// </summary>
    [HttpGet("account")]
    public async Task<ActionResult<IEnumerable<Campaign>>> GetCampaignsByAccount()
    {
        try
        {
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();
            
            _logger.LogInformation("Getting campaigns for account {AccountId}", accountId);
            var campaigns = await _campaignLogic.GetCampaignsByAccountAsync(accountId);
            
            _logger.LogInformation("Retrieved {Count} campaigns", campaigns.Count());
            return Ok(campaigns);
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
            _logger.LogError(ex, "Error retrieving campaigns");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific campaign by ID (must be owned by the authenticated user)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Campaign>> GetCampaign(int id)
    {
        try
        {
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

            var campaign = await _campaignLogic.GetCampaignAsync(id);
            if (campaign == null)
            {
                return NotFound($"Campaign with ID {id} not found");
            }

            // Verify the campaign belongs to the authenticated user's account
            if (campaign.account_id != accountId)
            {
                _logger.LogWarning("User attempted to access campaign {CampaignId} not owned by their account {AccountId}", id, accountId);
                return Forbid();
            }

            return Ok(campaign);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaign {CampaignId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new campaign for the authenticated user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> CreateCampaign([FromBody] Campaign campaign)
    {
        try
        {
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

            if (string.IsNullOrWhiteSpace(campaign.name))
            {
                return BadRequest("Campaign name is required");
            }

            if (campaign.game_system_id <= 0)
            {
                return BadRequest("Valid game system ID is required");
            }

            _logger.LogInformation("Creating new campaign: {Name} for account {AccountId}", campaign.name, accountId);
            
            int campaignId = await _campaignLogic.CreateCampaignAsync(
                accountId, 
                campaign.name, 
                campaign.description, 
                campaign.game_system_id);

            return CreatedAtAction(nameof(GetCampaign), new { id = campaignId }, campaignId);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing campaign (must be owned by the authenticated user)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCampaign(int id, [FromBody] Campaign campaign)
    {
        try
        {
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

            if (id != campaign.campaign_id)
            {
                return BadRequest("Campaign ID mismatch");
            }

            if (string.IsNullOrWhiteSpace(campaign.name))
            {
                return BadRequest("Campaign name is required");
            }

            if (campaign.game_system_id <= 0)
            {
                return BadRequest("Valid game system ID is required");
            }

            _logger.LogInformation("Updating campaign {CampaignId}", id);
            
            bool success = await _campaignLogic.UpdateCampaignAsync(
                id, 
                accountId, 
                campaign.name, 
                campaign.description, 
                campaign.game_system_id);

            if (!success)
            {
                return NotFound($"Campaign with ID {id} not found or not owned by your account");
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating campaign {CampaignId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a campaign (must be owned by the authenticated user)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCampaign(int id)
    {
        try
        {
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

            // First verify ownership
            var campaign = await _campaignLogic.GetCampaignAsync(id);
            if (campaign == null)
            {
                return NotFound($"Campaign with ID {id} not found");
            }

            if (campaign.account_id != accountId)
            {
                _logger.LogWarning("User attempted to delete campaign {CampaignId} not owned by their account {AccountId}", id, accountId);
                return Forbid();
            }

            _logger.LogInformation("Deleting campaign {CampaignId}", id);
            bool success = await _campaignLogic.DeleteCampaignAsync(id);

            if (!success)
            {
                return NotFound($"Campaign with ID {id} not found");
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting campaign {CampaignId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
