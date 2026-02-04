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
    private readonly IReferenceDataProvider _provider;
    private readonly IAuthHelper _authHelper;

    public ReferenceDataController(
        ILogger<ReferenceDataController> logger,
        IReferenceDataRepository repository,
        IReferenceDataProvider provider,
        IAuthHelper authHelper)
    {
        _logger = logger;
        _repository = repository;
        _provider = provider;
        _authHelper = authHelper;
    }

    // Lineage endpoints

    /// <summary>
    /// Get all lineages for a game system (SRD + optional user/campaign custom lineages)
    /// </summary>
    [HttpGet("{gameSystemId}/lineages")]
    public async Task<ActionResult<IEnumerable<ReferenceLineage>>> GetLineages(
        int gameSystemId,
        [FromQuery] int? accountId = null,
        [FromQuery] int? campaignId = null,
        CancellationToken ct = default)
    {
        try
        {
            var lineages = await _repository.GetLineagesAsync(gameSystemId, accountId, campaignId, ct);
            _logger.LogInformation("Retrieved {Count} lineages for game system {GameSystemId}", lineages.Count(), gameSystemId);
            return Ok(lineages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lineages for game system {GameSystemId}", gameSystemId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific lineage by ID
    /// </summary>
    [HttpGet("lineages/{lineageId}")]
    public async Task<ActionResult<ReferenceLineage>> GetLineage(int lineageId, CancellationToken ct = default)
    {
        try
        {
            var lineage = await _repository.GetLineageByIdAsync(lineageId, ct);
            if (lineage == null)
            {
                return NotFound($"Lineage with ID {lineageId} not found");
            }
            return Ok(lineage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lineage {LineageId}", lineageId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a custom lineage (must have account_id and campaign_id)
    /// </summary>
    [HttpPost("lineages")]
    public async Task<ActionResult<ReferenceLineage>> CreateLineage([FromBody] ReferenceLineage lineage, CancellationToken ct = default)
    {
        try
        {
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();
            
            // Ensure the lineage is tied to the authenticated user
            lineage.account_id = accountId;

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
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lineage");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a custom lineage (only user's own custom lineages)
    /// </summary>
    [HttpPut("lineages/{lineageId}")]
    public async Task<ActionResult> UpdateLineage(int lineageId, [FromBody] ReferenceLineage lineage, CancellationToken ct = default)
    {
        try
        {
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
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lineage {LineageId}", lineageId);
            return StatusCode(500, "Internal server error");
        }
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
        try
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
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lineage {LineageId}", lineageId);
            return StatusCode(500, "Internal server error");
        }
    }

    // Occupation endpoints

    /// <summary>
    /// Get all occupations for a game system (SRD + optional user/campaign custom occupations)
    /// </summary>
    [HttpGet("{gameSystemId}/occupations")]
    public async Task<ActionResult<IEnumerable<ReferenceOccupation>>> GetOccupations(
        int gameSystemId,
        [FromQuery] int? accountId = null,
        [FromQuery] int? campaignId = null,
        CancellationToken ct = default)
    {
        try
        {
            var occupations = await _repository.GetOccupationsAsync(gameSystemId, accountId, campaignId, ct);
            _logger.LogInformation("Retrieved {Count} occupations for game system {GameSystemId}", occupations.Count(), gameSystemId);
            return Ok(occupations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving occupations for game system {GameSystemId}", gameSystemId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific occupation by ID
    /// </summary>
    [HttpGet("occupations/{occupationId}")]
    public async Task<ActionResult<ReferenceOccupation>> GetOccupation(int occupationId, CancellationToken ct = default)
    {
        try
        {
            var occupation = await _repository.GetOccupationByIdAsync(occupationId, ct);
            if (occupation == null)
            {
                return NotFound($"Occupation with ID {occupationId} not found");
            }
            return Ok(occupation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving occupation {OccupationId}", occupationId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a custom occupation (must have account_id and campaign_id)
    /// </summary>
    [HttpPost("occupations")]
    public async Task<ActionResult<ReferenceOccupation>> CreateOccupation([FromBody] ReferenceOccupation occupation, CancellationToken ct = default)
    {
        try
        {
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();
            
            // Ensure the occupation is tied to the authenticated user
            occupation.account_id = accountId;

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
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating occupation");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a custom occupation (only user's own custom occupations)
    /// </summary>
    [HttpPut("occupations/{occupationId}")]
    public async Task<ActionResult> UpdateOccupation(int occupationId, [FromBody] ReferenceOccupation occupation, CancellationToken ct = default)
    {
        try
        {
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
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating occupation {OccupationId}", occupationId);
            return StatusCode(500, "Internal server error");
        }
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
        try
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
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting occupation {OccupationId}", occupationId);
            return StatusCode(500, "Internal server error");
        }
    }
}
