using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationsController : ControllerBase
{
    private readonly ILogger<OrganizationsController> _logger;
    private readonly IOrganizationRepository _repository;

    public OrganizationsController(ILogger<OrganizationsController> logger, IOrganizationRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    /// <summary>
    /// Get all organizations, optionally filtered by account
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Organization>>> GetOrganizations(
        [FromQuery] int? accountId = null)
    {
        try
        {
            if (!accountId.HasValue)
            {
                return BadRequest("Account ID is required");
            }

            _logger.LogInformation("Getting organizations for account {AccountId}", accountId);
            IEnumerable<Organization> organizations = await _repository.GetOrganizationsByAccountIdAsync(accountId.Value);
            return Ok(organizations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organizations");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific organization by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Organization>> GetOrganization(int id)
    {
        try
        {
            _logger.LogInformation("Getting organization {OrganizationId}", id);
            Organization? organization = await _repository.GetOrganizationByIdAsync(id);
            
            if (organization == null)
            {
                return NotFound($"Organization with ID {id} not found");
            }
            
            return Ok(organization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organization {OrganizationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new organization
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> CreateOrganization([FromBody] Organization organization)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating new organization: {Name}", organization.name);
            int organizationId = await _repository.CreateOrganizationAsync(organization);
            
            return CreatedAtAction(nameof(GetOrganization), new { id = organizationId }, organizationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organization");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing organization
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrganization(int id, [FromBody] Organization organization)
    {
        try
        {
            if (id != organization.organization_id)
            {
                return BadRequest("Organization ID mismatch");
            }

            if (!await _repository.OrganizationExistsAsync(id))
            {
                return NotFound($"Organization with ID {id} not found");
            }

            _logger.LogInformation("Updating organization {OrganizationId}", id);
            await _repository.UpdateOrganizationAsync(organization);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization {OrganizationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete an organization
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrganization(int id)
    {
        try
        {
            if (!await _repository.OrganizationExistsAsync(id))
            {
                return NotFound($"Organization with ID {id} not found");
            }

            _logger.LogInformation("Deleting organization {OrganizationId}", id);
            await _repository.DeleteOrganizationAsync(id);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting organization {OrganizationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all organizations for a specific account
    /// </summary>
    [HttpGet("account/{accountId}")]
    public async Task<ActionResult<IEnumerable<Organization>>> GetOrganizationsByAccount(int accountId)
    {
        try
        {
            _logger.LogInformation("Getting organizations for account {AccountId}", accountId);
            IEnumerable<Organization> organizations = await _repository.GetOrganizationsByAccountIdAsync(accountId);
            return Ok(organizations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organizations for account {AccountId}", accountId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all organizations in a specific campaign
    /// </summary>
    [HttpGet("campaign/{campaignId}")]
    public async Task<ActionResult<IEnumerable<Organization>>> GetOrganizationsByCampaign(int campaignId)
    {
        try
        {
            _logger.LogInformation("Getting organizations for campaign {CampaignId}", campaignId);
            IEnumerable<Organization> organizations = await _repository.GetOrganizationsByCampaignIdAsync(campaignId);
            return Ok(organizations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organizations for campaign {CampaignId}", campaignId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search organizations by name or type
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Organization>>> SearchOrganizations(
        [FromQuery] int accountId,
        [FromQuery] string? name = null,
        [FromQuery] string? type = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(type))
            {
                return BadRequest("At least one search parameter (name or type) is required");
            }

            _logger.LogInformation("Searching organizations");
            string searchTerm = name ?? type ?? "";
            IEnumerable<Organization> organizations = await _repository.SearchOrganizationsAsync(accountId, searchTerm);
            
            return Ok(organizations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching organizations");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all members of an organization (NPCs, PCs, etc.)
    /// </summary>
    [HttpGet("{id}/members")]
    public async Task<ActionResult<object>> GetOrganizationMembers(int id)
    {
        try
        {
            _logger.LogInformation("Getting members for organization {OrganizationId}", id);
            
            if (!await _repository.OrganizationExistsAsync(id))
            {
                return NotFound($"Organization with ID {id} not found");
            }

            // Return reference to relationships API
            return Ok(new 
            { 
                message = "Use /Relationships/to/organization/{id} endpoint to get members",
                endpoint = $"/Relationships/to/organization/{id}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving members for organization {OrganizationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all leaders of an organization
    /// </summary>
    [HttpGet("{id}/leaders")]
    public async Task<ActionResult<object>> GetOrganizationLeaders(int id)
    {
        try
        {
            _logger.LogInformation("Getting leaders for organization {OrganizationId}", id);
            
            if (!await _repository.OrganizationExistsAsync(id))
            {
                return NotFound($"Organization with ID {id} not found");
            }

            return Ok(new 
            { 
                message = "Use /Relationships/to/organization/{id} with type filter",
                endpoint = $"/Relationships/entity/organization/{id}/type/8"  // 8 is Leader relationship type
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leaders for organization {OrganizationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get the organizational hierarchy
    /// </summary>
    [HttpGet("{id}/hierarchy")]
    public async Task<ActionResult<object>> GetOrganizationHierarchy(int id)
    {
        try
        {
            _logger.LogInformation("Getting hierarchy for organization {OrganizationId}", id);
            
            if (!await _repository.OrganizationExistsAsync(id))
            {
                return NotFound($"Organization with ID {id} not found");
            }

            // TODO: Build hierarchy tree from relationships
            return Ok(new { message = "Hierarchy not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hierarchy for organization {OrganizationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all allied organizations
    /// </summary>
    [HttpGet("{id}/allies")]
    public async Task<ActionResult<object>> GetAlliedOrganizations(int id)
    {
        try
        {
            _logger.LogInformation("Getting allies for organization {OrganizationId}", id);
            
            if (!await _repository.OrganizationExistsAsync(id))
            {
                return NotFound($"Organization with ID {id} not found");
            }

            return Ok(new 
            { 
                message = "Use /Relationships/from/organization/{id} with type=Ally filter",
                endpoint = $"/Relationships/entity/organization/{id}/type/2"  // 2 is Ally relationship type
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving allies for organization {OrganizationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all enemy organizations
    /// </summary>
    [HttpGet("{id}/enemies")]
    public async Task<ActionResult<object>> GetEnemyOrganizations(int id)
    {
        try
        {
            _logger.LogInformation("Getting enemies for organization {OrganizationId}", id);
            
            if (!await _repository.OrganizationExistsAsync(id))
            {
                return NotFound($"Organization with ID {id} not found");
            }

            return Ok(new 
            { 
                message = "Use /Relationships/from/organization/{id} with type=Enemy filter",
                endpoint = $"/Relationships/entity/organization/{id}/type/3"  // 3 is Enemy relationship type
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving enemies for organization {OrganizationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Export organizations in specified format
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportOrganizations(
        [FromQuery] int accountId,
        [FromQuery] string format = "json")
    {
        try
        {
            _logger.LogInformation("Exporting organizations for account {AccountId} in {Format} format", 
                accountId, format);
            
            IEnumerable<Organization> organizations = await _repository.GetOrganizationsByAccountIdAsync(accountId);
            
            // For now, just return JSON
            // TODO: Add support for other formats
            return Ok(organizations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting organizations");
            return StatusCode(500, "Internal server error");
        }
    }
}
