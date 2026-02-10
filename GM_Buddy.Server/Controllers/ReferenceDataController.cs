using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReferenceDataController : ControllerBase
{
    private readonly ILogger<ReferenceDataController> _logger;
    private readonly IReferenceDataRepository _repository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IAuthHelper _authHelper;

    public ReferenceDataController(
        ILogger<ReferenceDataController> logger,
        IReferenceDataRepository repository,
        ICampaignRepository campaignRepository,
        IAuthHelper authHelper)
    {
        _logger = logger;
        _repository = repository;
        _campaignRepository = campaignRepository;
        _authHelper = authHelper;
    }

    // Lineage endpoints

    /// <summary>
    /// Get all lineages for a game system (SRD + optional user/campaign custom lineages)
    /// </summary>
    [HttpGet("{gameSystemId}/lineages")]
    public async Task<ActionResult<IEnumerable<ReferenceLineage>>> GetLineages(
        int gameSystemId,
        [FromQuery] int? campaignId = null,
        CancellationToken ct = default)
    {
        int? accountId = null;

        // If campaign is specified, get authenticated account ID
        if (campaignId.HasValue)
        {
            try
            {
                accountId = await _authHelper.GetAuthenticatedAccountIdAsync();
            }
            catch (UnauthorizedAccessException)
            {
                // User not authenticated, only return SRD content
                _logger.LogInformation("Unauthenticated request for campaign {CampaignId}, returning SRD content only", campaignId);
                accountId = null;
                campaignId = null;
            }
        }

        var lineages = await _repository.GetLineagesAsync(gameSystemId, accountId, campaignId, ct);
        _logger.LogInformation("Retrieved {Count} lineages for game system {GameSystemId}", lineages.Count(), gameSystemId);
        return Ok(lineages);
    }

    /// <summary>
    /// Get a specific lineage by ID
    /// </summary>
    [HttpGet("lineages/{lineageId}")]
    public async Task<ActionResult<ReferenceLineage>> GetLineage(int lineageId, CancellationToken ct = default)
    {
        var lineage = await _repository.GetLineageByIdAsync(lineageId, ct);
        if (lineage == null)
        {
            return NotFound($"Lineage with ID {lineageId} not found");
        }

        // Authorization check: Only allow access to SRD or user's own custom lineages
        if (lineage.account_id != null)
        {
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();
            if (lineage.account_id != accountId)
            {
                _logger.LogWarning("User {AccountId} attempted to access lineage {LineageId} owned by {OwnerId}",
                    accountId, lineageId, lineage.account_id);
                return Forbid();
            }
        }

        return Ok(lineage);
    }

    /// <summary>
    /// Create a custom lineage (must have account_id and campaign_id)
    /// </summary>
    [HttpPost("lineages")]
    public async Task<ActionResult<ReferenceLineage>> CreateLineage([FromBody] ReferenceLineage lineage, CancellationToken ct = default)
    {
        // Validate input
        if (lineage == null)
        {
            return BadRequest("Lineage payload is required.");
        }

        if (string.IsNullOrWhiteSpace(lineage.name))
        {
            return BadRequest("Lineage name is required.");
        }

        if (lineage.game_system_id <= 0)
        {
            return BadRequest("A valid game system id is required.");
        }

        if (!lineage.campaign_id.HasValue || lineage.campaign_id.Value <= 0)
        {
            return BadRequest("A valid campaign_id is required for custom lineages.");
        }

        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        // Ensure the lineage is tied to the authenticated user
        lineage.account_id = accountId;

        // Validate that the campaign exists and belongs to the user
        var campaign = await _campaignRepository.GetByIdAsync(lineage.campaign_id.Value, ct);
        if (campaign == null)
        {
            _logger.LogWarning("Attempt to create lineage for non-existent campaign {CampaignId} by account {AccountId}",
                lineage.campaign_id, accountId);
            return BadRequest("The specified campaign does not exist.");
        }

        if (campaign.account_id != accountId)
        {
            _logger.LogWarning("Account {AccountId} attempted to create lineage for campaign {CampaignId} owned by {OwnerId}",
                accountId, lineage.campaign_id, campaign.account_id);
            return Forbid();
        }

        // Validate that the lineage's game_system_id matches the campaign's game system
        if (campaign.game_system_id != lineage.game_system_id)
        {
            _logger.LogWarning(
                "Game system mismatch when creating lineage for campaign {CampaignId}: campaign game_system_id {CampaignGameSystemId}, lineage game_system_id {LineageGameSystemId}",
                lineage.campaign_id,
                campaign.game_system_id,
                lineage.game_system_id);
            return BadRequest("The lineage's game system does not match the campaign's game system.");
        }

        int lineageId = await _repository.CreateLineageAsync(lineage, ct);
        _logger.LogInformation("Created custom lineage {LineageId} for account {AccountId}, campaign {CampaignId}",
            lineageId, accountId, lineage.campaign_id);

        var createdLineage = await _repository.GetLineageByIdAsync(lineageId, ct);
        if (createdLineage == null)
        {
            return StatusCode(500, "Lineage was created but could not be retrieved");
        }

        return CreatedAtAction(nameof(GetLineage), new { lineageId }, createdLineage);
    }

    /// <summary>
    /// Update a custom lineage (only user's own custom lineages)
    /// </summary>
    [HttpPut("lineages/{lineageId}")]
    public async Task<ActionResult> UpdateLineage(int lineageId, [FromBody] ReferenceLineage lineage, CancellationToken ct = default)
    {
        // Validate ID match
        if (lineage.lineage_id != 0 && lineage.lineage_id != lineageId)
        {
            return BadRequest("Lineage ID in route must match lineage ID in body.");
        }

        // Validate required fields
        if (!lineage.campaign_id.HasValue)
        {
            return BadRequest("campaign_id is required to update a custom lineage.");
        }

        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        // Ensure the lineage ID matches and belongs to the authenticated user
        lineage.lineage_id = lineageId;
        lineage.account_id = accountId;

        bool success = await _repository.UpdateLineageAsync(lineage, ct);
        if (!success)
        {
            return NotFound($"Lineage with ID {lineageId} not found or not owned by your account");
        }

        _logger.LogInformation("Updated custom lineage {LineageId}", lineageId);
        return NoContent();
    }

    /// <summary>
    /// Delete a custom lineage (only user's own custom lineages)
    /// </summary>
    [HttpDelete("lineages/{lineageId}")]
    public async Task<ActionResult> DeleteLineage(
        int lineageId,
        [FromQuery] int? campaignId = null,
        CancellationToken ct = default)
    {
        int authenticatedAccountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        // Use the authenticated account ID for deletion
        bool success = await _repository.DeleteLineageAsync(lineageId, authenticatedAccountId, campaignId, ct);
        if (!success)
        {
            return NotFound($"Lineage with ID {lineageId} not found or not owned by your account");
        }

        _logger.LogInformation("Deleted custom lineage {LineageId}", lineageId);
        return NoContent();
    }

    // Occupation endpoints

    /// <summary>
    /// Get all occupations for a game system (SRD + optional user/campaign custom occupations)
    /// </summary>
    [HttpGet("{gameSystemId}/occupations")]
    public async Task<ActionResult<IEnumerable<ReferenceOccupation>>> GetOccupations(
        int gameSystemId,
        [FromQuery] int? campaignId = null,
        CancellationToken ct = default)
    {
        int? accountId = null;

        // If campaign is specified, get authenticated account ID
        if (campaignId.HasValue)
        {
            try
            {
                accountId = await _authHelper.GetAuthenticatedAccountIdAsync();
            }
            catch (UnauthorizedAccessException)
            {
                // User not authenticated, only return SRD content
                _logger.LogInformation("Unauthenticated request for campaign {CampaignId}, returning SRD content only", campaignId);
                accountId = null;
                campaignId = null;
            }
        }

        var occupations = await _repository.GetOccupationsAsync(gameSystemId, accountId, campaignId, ct);
        _logger.LogInformation("Retrieved {Count} occupations for game system {GameSystemId}", occupations.Count(), gameSystemId);
        return Ok(occupations);
    }

    /// <summary>
    /// Get a specific occupation by ID
    /// </summary>
    [HttpGet("occupations/{occupationId}")]
    public async Task<ActionResult<ReferenceOccupation>> GetOccupation(int occupationId, CancellationToken ct = default)
    {
        var occupation = await _repository.GetOccupationByIdAsync(occupationId, ct);
        if (occupation == null)
        {
            return NotFound($"Occupation with ID {occupationId} not found");
        }

        // Authorization check: Only allow access to SRD or user's own custom occupations
        if (occupation.account_id != null)
        {
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();
            if (occupation.account_id != accountId)
            {
                _logger.LogWarning("User {AccountId} attempted to access occupation {OccupationId} owned by {OwnerId}",
                    accountId, occupationId, occupation.account_id);
                return Forbid();
            }
        }

        return Ok(occupation);
    }

    /// <summary>
    /// Create a custom occupation (must have account_id and campaign_id)
    /// </summary>
    [HttpPost("occupations")]
    public async Task<ActionResult<ReferenceOccupation>> CreateOccupation([FromBody] ReferenceOccupation occupation, CancellationToken ct = default)
    {
        // Validate input
        if (occupation == null)
        {
            return BadRequest("Occupation payload is required.");
        }

        if (string.IsNullOrWhiteSpace(occupation.name))
        {
            return BadRequest("Occupation name is required.");
        }

        if (occupation.game_system_id <= 0)
        {
            return BadRequest("A valid game system id is required.");
        }

        if (!occupation.campaign_id.HasValue || occupation.campaign_id.Value <= 0)
        {
            return BadRequest("A valid campaign_id is required for custom occupations.");
        }

        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        // Ensure the occupation is tied to the authenticated user
        occupation.account_id = accountId;

        // Validate that the campaign exists and belongs to the user
        var campaign = await _campaignRepository.GetByIdAsync(occupation.campaign_id.Value, ct);
        if (campaign == null)
        {
            _logger.LogWarning("Attempt to create occupation for non-existent campaign {CampaignId} by account {AccountId}",
                occupation.campaign_id, accountId);
            return BadRequest("The specified campaign does not exist.");
        }

        if (campaign.account_id != accountId)
        {
            _logger.LogWarning("Account {AccountId} attempted to create occupation for campaign {CampaignId} owned by {OwnerId}",
                accountId, occupation.campaign_id, campaign.account_id);
            return Forbid();
        }

        // Validate that the occupation's game_system_id matches the campaign's game system
        if (campaign.game_system_id != occupation.game_system_id)
        {
            _logger.LogWarning(
                "Game system mismatch when creating occupation for campaign {CampaignId}: campaign game_system_id {CampaignGameSystemId}, occupation game_system_id {OccupationGameSystemId}",
                occupation.campaign_id,
                campaign.game_system_id,
                occupation.game_system_id);
            return BadRequest("The occupation's game system does not match the campaign's game system.");
        }

        int occupationId = await _repository.CreateOccupationAsync(occupation, ct);
        _logger.LogInformation("Created custom occupation {OccupationId} for account {AccountId}, campaign {CampaignId}",
            occupationId, accountId, occupation.campaign_id);

        var createdOccupation = await _repository.GetOccupationByIdAsync(occupationId, ct);
        if (createdOccupation == null)
        {
            return StatusCode(500, "Occupation was created but could not be retrieved");
        }

        return CreatedAtAction(nameof(GetOccupation), new { occupationId }, createdOccupation);
    }

    /// <summary>
    /// Update a custom occupation (only user's own custom occupations)
    /// </summary>
    [HttpPut("occupations/{occupationId}")]
    public async Task<ActionResult> UpdateOccupation(int occupationId, [FromBody] ReferenceOccupation occupation, CancellationToken ct = default)
    {
        // Validate ID match
        if (occupation.occupation_id != 0 && occupation.occupation_id != occupationId)
        {
            return BadRequest("Occupation ID in route must match occupation ID in body.");
        }

        // Validate required fields
        if (!occupation.campaign_id.HasValue)
        {
            return BadRequest("campaign_id is required to update a custom occupation.");
        }

        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        // Ensure the occupation ID matches and belongs to the authenticated user
        occupation.occupation_id = occupationId;
        occupation.account_id = accountId;

        bool success = await _repository.UpdateOccupationAsync(occupation, ct);
        if (!success)
        {
            return NotFound($"Occupation with ID {occupationId} not found or not owned by your account");
        }

        _logger.LogInformation("Updated custom occupation {OccupationId}", occupationId);
        return NoContent();
    }

    /// <summary>
    /// Delete a custom occupation (only user's own custom occupations)
    /// </summary>
    [HttpDelete("occupations/{occupationId}")]
    public async Task<ActionResult> DeleteOccupation(
        int occupationId, 
        [FromQuery] int? campaignId = null, 
        CancellationToken ct = default)
    {
        int authenticatedAccountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        // Use the authenticated account ID for deletion
        bool success = await _repository.DeleteOccupationAsync(occupationId, authenticatedAccountId, campaignId, ct);
        if (!success)
        {
            return NotFound($"Occupation with ID {occupationId} not found or not owned by your account");
        }

        _logger.LogInformation("Deleted custom occupation {OccupationId}", occupationId);
        return NoContent();
    }
}
