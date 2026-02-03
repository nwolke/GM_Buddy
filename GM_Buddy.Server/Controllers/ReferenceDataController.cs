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

    // Race endpoints

    /// <summary>
    /// Get all races for a game system (SRD + optional user custom races)
    /// </summary>
    [HttpGet("{gameSystemId}/races")]
    public async Task<ActionResult<IEnumerable<ReferenceRace>>> GetRaces(
        int gameSystemId,
        [FromQuery] int? accountId = null,
        CancellationToken ct = default)
    {
        try
        {
            var races = await _repository.GetRacesAsync(gameSystemId, accountId, ct);
            _logger.LogInformation("Retrieved {Count} races for game system {GameSystemId}", races.Count(), gameSystemId);
            return Ok(races);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving races for game system {GameSystemId}", gameSystemId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific race by ID
    /// </summary>
    [HttpGet("races/{raceId}")]
    public async Task<ActionResult<ReferenceRace>> GetRace(int raceId, CancellationToken ct = default)
    {
        try
        {
            var race = await _repository.GetRaceByIdAsync(raceId, ct);
            if (race == null)
            {
                return NotFound($"Race with ID {raceId} not found");
            }
            return Ok(race);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving race {RaceId}", raceId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a custom race (must have account_id)
    /// </summary>
    [HttpPost("races")]
    public async Task<ActionResult<ReferenceRace>> CreateRace([FromBody] ReferenceRace race, CancellationToken ct = default)
    {
        try
        {
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();
            
            // Ensure the race is tied to the authenticated user
            race.account_id = accountId;

            int raceId = await _repository.CreateRaceAsync(race, ct);
            _logger.LogInformation("Created custom race {RaceId} for account {AccountId}", raceId, accountId);

            var createdRace = await _repository.GetRaceByIdAsync(raceId, ct);
            if (createdRace == null)
            {
                return StatusCode(500, "Race was created but could not be retrieved");
            }

            return CreatedAtAction(nameof(GetRace), new { raceId }, createdRace);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating race");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a custom race (only user's own custom races)
    /// </summary>
    [HttpPut("races/{raceId}")]
    public async Task<ActionResult> UpdateRace(int raceId, [FromBody] ReferenceRace race, CancellationToken ct = default)
    {
        try
        {
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();
            
            // Ensure the race ID matches and belongs to the authenticated user
            race.race_id = raceId;
            race.account_id = accountId;

            bool success = await _repository.UpdateRaceAsync(race, ct);
            if (!success)
            {
                return NotFound($"Race with ID {raceId} not found or not owned by your account");
            }

            _logger.LogInformation("Updated custom race {RaceId}", raceId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating race {RaceId}", raceId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a custom race (only user's own custom races)
    /// </summary>
    [HttpDelete("races/{raceId}")]
    public async Task<ActionResult> DeleteRace(int raceId, CancellationToken ct = default)
    {
        try
        {
            int authenticatedAccountId = await _authHelper.GetAuthenticatedAccountIdAsync();
            
            // Use the authenticated account ID for deletion
            bool success = await _repository.DeleteRaceAsync(raceId, authenticatedAccountId, ct);
            if (!success)
            {
                return NotFound($"Race with ID {raceId} not found or not owned by your account");
            }

            _logger.LogInformation("Deleted custom race {RaceId}", raceId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting race {RaceId}", raceId);
            return StatusCode(500, "Internal server error");
        }
    }

    // Class endpoints

    /// <summary>
    /// Get all classes for a game system (SRD + optional user custom classes)
    /// </summary>
    [HttpGet("{gameSystemId}/classes")]
    public async Task<ActionResult<IEnumerable<ReferenceClass>>> GetClasses(
        int gameSystemId,
        [FromQuery] int? accountId = null,
        CancellationToken ct = default)
    {
        try
        {
            var classes = await _repository.GetClassesAsync(gameSystemId, accountId, ct);
            _logger.LogInformation("Retrieved {Count} classes for game system {GameSystemId}", classes.Count(), gameSystemId);
            return Ok(classes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving classes for game system {GameSystemId}", gameSystemId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific class by ID
    /// </summary>
    [HttpGet("classes/{classId}")]
    public async Task<ActionResult<ReferenceClass>> GetClass(int classId, CancellationToken ct = default)
    {
        try
        {
            var referenceClass = await _repository.GetClassByIdAsync(classId, ct);
            if (referenceClass == null)
            {
                return NotFound($"Class with ID {classId} not found");
            }
            return Ok(referenceClass);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving class {ClassId}", classId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a custom class (must have account_id)
    /// </summary>
    [HttpPost("classes")]
    public async Task<ActionResult<ReferenceClass>> CreateClass([FromBody] ReferenceClass referenceClass, CancellationToken ct = default)
    {
        try
        {
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();
            
            // Ensure the class is tied to the authenticated user
            referenceClass.account_id = accountId;

            int classId = await _repository.CreateClassAsync(referenceClass, ct);
            _logger.LogInformation("Created custom class {ClassId} for account {AccountId}", classId, accountId);

            var createdClass = await _repository.GetClassByIdAsync(classId, ct);
            if (createdClass == null)
            {
                return StatusCode(500, "Class was created but could not be retrieved");
            }

            return CreatedAtAction(nameof(GetClass), new { classId }, createdClass);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating class");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a custom class (only user's own custom classes)
    /// </summary>
    [HttpPut("classes/{classId}")]
    public async Task<ActionResult> UpdateClass(int classId, [FromBody] ReferenceClass referenceClass, CancellationToken ct = default)
    {
        try
        {
            int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();
            
            // Ensure the class ID matches and belongs to the authenticated user
            referenceClass.class_id = classId;
            referenceClass.account_id = accountId;

            bool success = await _repository.UpdateClassAsync(referenceClass, ct);
            if (!success)
            {
                return NotFound($"Class with ID {classId} not found or not owned by your account");
            }

            _logger.LogInformation("Updated custom class {ClassId}", classId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating class {ClassId}", classId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a custom class (only user's own custom classes)
    /// </summary>
    [HttpDelete("classes/{classId}")]
    public async Task<ActionResult> DeleteClass(int classId, CancellationToken ct = default)
    {
        try
        {
            int authenticatedAccountId = await _authHelper.GetAuthenticatedAccountIdAsync();
            
            // Use the authenticated account ID for deletion
            bool success = await _repository.DeleteClassAsync(classId, authenticatedAccountId, ct);
            if (!success)
            {
                return NotFound($"Class with ID {classId} not found or not owned by your account");
            }

            _logger.LogInformation("Deleted custom class {ClassId}", classId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting class {ClassId}", classId);
            return StatusCode(500, "Internal server error");
        }
    }
}
