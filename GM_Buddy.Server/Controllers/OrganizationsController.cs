using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganizationsController : ControllerBase
{
    private readonly ILogger<OrganizationsController> _logger;
    private readonly IOrganizationRepository _repository;
    private readonly IAuthHelper _authHelper;
    private readonly ICampaignLogic _campaignLogic;

    public OrganizationsController(
        ILogger<OrganizationsController> logger,
        IOrganizationRepository repository,
        IAuthHelper authHelper,
        ICampaignLogic campaignLogic)
    {
        _logger = logger;
        _repository = repository;
        _authHelper = authHelper;
        _campaignLogic = campaignLogic;
    }

    /// <summary>
    /// Get all organizations for the authenticated account
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Organization>>> GetOrganizations()
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        _logger.LogInformation("Getting organizations for account {AccountId}", accountId);
        IEnumerable<Organization> organizations = await _repository.GetOrganizationsByAccountIdAsync(accountId);
        return Ok(organizations);
    }

    /// <summary>
    /// Get a specific organization by ID (must belong to authenticated account)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Organization>> GetOrganization(int id)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        _logger.LogInformation("Getting organization {OrganizationId} for account {AccountId}", id, accountId);
        Organization? organization = await _repository.GetOrganizationByIdAsync(id);

        if (organization == null || organization.account_id != accountId)
        {
            return NotFound($"Organization with ID {id} not found");
        }

        return Ok(organization);
    }

    /// <summary>
    /// Create a new organization for the authenticated account
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> CreateOrganization([FromBody] Organization organization)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Override client-supplied account_id with authenticated account
        organization.account_id = accountId;

        _logger.LogInformation("Creating new organization: {Name} for account {AccountId}", organization.name, accountId);
        int organizationId = await _repository.CreateOrganizationAsync(organization);

        return CreatedAtAction(nameof(GetOrganization), new { id = organizationId }, organizationId);
    }

    /// <summary>
    /// Update an existing organization (must belong to authenticated account)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrganization(int id, [FromBody] Organization organization)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        if (id != organization.organization_id)
        {
            return BadRequest("Organization ID mismatch");
        }

        // Verify ownership
        Organization? existing = await _repository.GetOrganizationByIdAsync(id);
        if (existing == null || existing.account_id != accountId)
        {
            return NotFound($"Organization with ID {id} not found");
        }

        // Ensure account_id stays consistent
        organization.account_id = accountId;

        _logger.LogInformation("Updating organization {OrganizationId} for account {AccountId}", id, accountId);
        await _repository.UpdateOrganizationAsync(organization);

        return NoContent();
    }

    /// <summary>
    /// Delete an organization (must belong to authenticated account)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrganization(int id)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        // Verify ownership
        Organization? existing = await _repository.GetOrganizationByIdAsync(id);
        if (existing == null || existing.account_id != accountId)
        {
            return NotFound($"Organization with ID {id} not found");
        }

        _logger.LogInformation("Deleting organization {OrganizationId} for account {AccountId}", id, accountId);
        await _repository.DeleteOrganizationAsync(id);

        return NoContent();
    }

    /// <summary>
    /// Get all organizations for the authenticated account
    /// Returns 404 if route accountId doesn't match authenticated user
    /// </summary>
    [HttpGet("account/{accountId}")]
    public async Task<ActionResult<IEnumerable<Organization>>> GetOrganizationsByAccount(int accountId)
    {
        int authenticatedAccountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        if (authenticatedAccountId != accountId)
        {
            return NotFound("Account not found");
        }

        _logger.LogInformation("Getting organizations for account {AccountId}", authenticatedAccountId);
        IEnumerable<Organization> organizations = await _repository.GetOrganizationsByAccountIdAsync(authenticatedAccountId);
        return Ok(organizations);
    }

    /// <summary>
    /// Get all organizations in a specific campaign (must own the campaign)
    /// </summary>
    [HttpGet("campaign/{campaignId}")]
    public async Task<ActionResult<IEnumerable<Organization>>> GetOrganizationsByCampaign(int campaignId)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        // Verify campaign ownership
        var campaign = await _campaignLogic.GetCampaignAsync(campaignId, accountId);
        if (campaign == null)
        {
            return NotFound("Campaign not found");
        }

        _logger.LogInformation("Getting organizations for campaign {CampaignId} (account {AccountId})", campaignId, accountId);
        IEnumerable<Organization> organizations = await _repository.GetOrganizationsByCampaignIdAsync(campaignId);
        return Ok(organizations);
    }

    /// <summary>
    /// Search organizations by name or type for the authenticated account
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Organization>>> SearchOrganizations(
        [FromQuery] string? name = null,
        [FromQuery] string? type = null)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(type))
        {
            return BadRequest("At least one search parameter (name or type) is required");
        }

        _logger.LogInformation("Searching organizations for account {AccountId}", accountId);
        string searchTerm = name ?? type ?? "";
        IEnumerable<Organization> organizations = await _repository.SearchOrganizationsAsync(accountId, searchTerm);

        return Ok(organizations);
    }

    /// <summary>
    /// Get all members of an organization (NPCs, PCs, etc.)
    /// </summary>
    [HttpGet("{id}/members")]
    public async Task<ActionResult<object>> GetOrganizationMembers(int id)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        // Verify ownership
        Organization? organization = await _repository.GetOrganizationByIdAsync(id);
        if (organization == null || organization.account_id != accountId)
        {
            return NotFound($"Organization with ID {id} not found");
        }

        // Return reference to relationships API
        return Ok(new
        {
            message = "Use /api/Relationships/to/organization/{id} endpoint to get members",
            endpoint = $"/api/Relationships/to/organization/{id}"
        });
    }

    /// <summary>
    /// Get all leaders of an organization
    /// </summary>
    [HttpGet("{id}/leaders")]
    public async Task<ActionResult<object>> GetOrganizationLeaders(int id)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        // Verify ownership
        Organization? organization = await _repository.GetOrganizationByIdAsync(id);
        if (organization == null || organization.account_id != accountId)
        {
            return NotFound($"Organization with ID {id} not found");
        }

        return Ok(new
        {
            message = "Use /api/Relationships/entity/organization/{id}/type/8 to get leaders",
            endpoint = $"/api/Relationships/entity/organization/{id}/type/8"  // 8 is Leader relationship type
        });
    }

    /// <summary>
    /// Get the organizational hierarchy
    /// </summary>
    [HttpGet("{id}/hierarchy")]
    public async Task<ActionResult<object>> GetOrganizationHierarchy(int id)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        // Verify ownership
        Organization? organization = await _repository.GetOrganizationByIdAsync(id);
        if (organization == null || organization.account_id != accountId)
        {
            return NotFound($"Organization with ID {id} not found");
        }

        // TODO: Build hierarchy tree from relationships
        return Ok(new { message = "Hierarchy not yet implemented" });
    }

    /// <summary>
    /// Get all allied organizations
    /// </summary>
    [HttpGet("{id}/allies")]
    public async Task<ActionResult<object>> GetAlliedOrganizations(int id)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        // Verify ownership
        Organization? organization = await _repository.GetOrganizationByIdAsync(id);
        if (organization == null || organization.account_id != accountId)
        {
            return NotFound($"Organization with ID {id} not found");
        }

        return Ok(new
        {
            message = "Use /api/Relationships/entity/organization/{id}/type/2 to get allies",
            endpoint = $"/api/Relationships/entity/organization/{id}/type/2"  // 2 is Ally relationship type
        });
    }

    /// <summary>
    /// Get all enemy organizations
    /// </summary>
    [HttpGet("{id}/enemies")]
    public async Task<ActionResult<object>> GetEnemyOrganizations(int id)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        // Verify ownership
        Organization? organization = await _repository.GetOrganizationByIdAsync(id);
        if (organization == null || organization.account_id != accountId)
        {
            return NotFound($"Organization with ID {id} not found");
        }

        return Ok(new
        {
            message = "Use /api/Relationships/entity/organization/{id}/type/3 to get enemies",
            endpoint = $"/api/Relationships/entity/organization/{id}/type/3"  // 3 is Enemy relationship type
        });
    }

    /// <summary>
    /// Export organizations for the authenticated account
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportOrganizations(
        [FromQuery] string format = "json")
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        _logger.LogInformation("Exporting organizations for account {AccountId} in {Format} format",
            accountId, format);

        IEnumerable<Organization> organizations = await _repository.GetOrganizationsByAccountIdAsync(accountId);

        // For now, just return JSON
        // TODO: Add support for other formats
        return Ok(organizations);
    }
}
