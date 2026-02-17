using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Contracts.Models.Pcs;
using GM_Buddy.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PcsController : ControllerBase
{
    private readonly ILogger<PcsController> _logger;
    private readonly IPcLogic _logic;
    private readonly IAuthHelper _authHelper;

    public PcsController(
        ILogger<PcsController> logger,
        IPcLogic logic,
        IAuthHelper authHelper)
    {
        _logger = logger;
        _logic = logic;
        _authHelper = authHelper;
    }

    /// <summary>
    /// Get all PCs for the authenticated account
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PcDto>>> GetPcs()
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();
        IEnumerable<PcDto> pcs = await _logic.GetPcsAsync(accountId);
        return Ok(pcs);
    }

    /// <summary>
    /// Get a specific PC by ID (must be owned by the authenticated user)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PcDto>> GetPc(int id)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();
        _logger.LogInformation("Getting PC {PcId} for account {AccountId}", id, accountId);

        try
        {
            PcDto? pc = await _logic.GetPcAsync(id, accountId);
            if (pc == null)
            {
                return NotFound($"PC with ID {id} not found");
            }
            return Ok(pc);
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Account {AccountId} attempted to access PC {PcId} not owned by their account",
                accountId, id);
            return Forbid();
        }
    }

    /// <summary>
    /// Create a new PC for the authenticated user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PcDto>> CreatePc([FromBody] CreatePcRequest request)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating new PC: {Name} for account {AccountId}", request.Name, accountId);
        PcDto created = await _logic.CreatePcAsync(accountId, request);

        return CreatedAtAction(nameof(GetPc), new { id = created.Pc_Id }, created);
    }

    /// <summary>
    /// Update an existing PC (must be owned by the authenticated user)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePc(int id, [FromBody] UpdatePcRequest request)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            bool found = await _logic.UpdatePcAsync(id, accountId, request);
            if (!found)
            {
                return NotFound($"PC with ID {id} not found");
            }
            _logger.LogInformation("Updated PC {PcId} for account {AccountId}", id, accountId);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Account {AccountId} attempted to update PC {PcId} not owned by their account",
                accountId, id);
            return Forbid();
        }
    }

    /// <summary>
    /// Delete a PC (must be owned by the authenticated user)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePc(int id)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        try
        {
            bool found = await _logic.DeletePcAsync(id, accountId);
            if (!found)
            {
                return NotFound($"PC with ID {id} not found");
            }
            _logger.LogInformation("Deleted PC {PcId} for account {AccountId}", id, accountId);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Account {AccountId} attempted to delete PC {PcId} not owned by their account",
                accountId, id);
            return Forbid();
        }
    }

    /// <summary>
    /// Get all PCs in a specific campaign (linked via entity_relationship)
    /// User must own the campaign to access its PCs
    /// </summary>
    [HttpGet("campaign/{campaignId}")]
    public async Task<ActionResult<IEnumerable<PcDto>>> GetPcsByCampaign(int campaignId)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();
        _logger.LogInformation("Getting PCs for campaign {CampaignId} requested by account {AccountId}", 
            campaignId, accountId);
        
        // TODO: Verify campaign ownership - for now we'll allow access
        // In the future, add campaign ownership check here
        IEnumerable<PcDto> pcs = await _logic.GetPcsByCampaignAsync(campaignId);
        return Ok(pcs);
    }

    /// <summary>
    /// Get all PCs for a specific account
    /// User must be querying their own account
    /// </summary>
    [HttpGet("account/{accountId}")]
    public async Task<ActionResult<IEnumerable<PcDto>>> GetPcsByAccount(int accountId)
    {
        int authenticatedAccountId = await _authHelper.GetAuthenticatedAccountIdAsync();
        
        if (authenticatedAccountId != accountId)
        {
            _logger.LogWarning("Account {AuthAccountId} attempted to access PCs for account {AccountId}",
                authenticatedAccountId, accountId);
            return Forbid();
        }
        
        _logger.LogInformation("Getting PCs for account {AccountId}", accountId);
        IEnumerable<PcDto> pcs = await _logic.GetPcsAsync(accountId);
        return Ok(pcs);
    }

    /// <summary>
    /// Get all active PCs for an account
    /// User must be querying their own account
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<PcDto>>> GetActivePcs([FromQuery] int accountId)
    {
        int authenticatedAccountId = await _authHelper.GetAuthenticatedAccountIdAsync();
        
        if (authenticatedAccountId != accountId)
        {
            _logger.LogWarning("Account {AuthAccountId} attempted to access active PCs for account {AccountId}",
                authenticatedAccountId, accountId);
            return Forbid();
        }
        
        _logger.LogInformation("Getting active PCs for account {AccountId}", accountId);
        // TODO: Filter to only PCs currently in active campaigns
        IEnumerable<PcDto> pcs = await _logic.GetPcsAsync(accountId);
        return Ok(pcs);
    }

    /// <summary>
    /// Get the party (all PCs) for a campaign
    /// User must own the campaign to access its party
    /// </summary>
    [HttpGet("party/{campaignId}")]
    public async Task<ActionResult<IEnumerable<PcDto>>> GetParty(int campaignId)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();
        _logger.LogInformation("Getting party for campaign {CampaignId} requested by account {AccountId}", 
            campaignId, accountId);
        
        // TODO: Verify campaign ownership - for now we'll allow access
        // In the future, add campaign ownership check here
        IEnumerable<PcDto> pcs = await _logic.GetPcsByCampaignAsync(campaignId);
        return Ok(pcs);
    }

    /// <summary>
    /// Export PCs in specified format
    /// User must be exporting their own account's PCs
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportPcs(
        [FromQuery] int accountId,
        [FromQuery] string format = "json")
    {
        int authenticatedAccountId = await _authHelper.GetAuthenticatedAccountIdAsync();
        
        if (authenticatedAccountId != accountId)
        {
            _logger.LogWarning("Account {AuthAccountId} attempted to export PCs for account {AccountId}",
                authenticatedAccountId, accountId);
            return Forbid();
        }
        
        _logger.LogInformation("Exporting PCs for account {AccountId} in {Format} format",
            accountId, format);

        IEnumerable<PcDto> pcs = await _logic.GetPcsAsync(accountId);

        // TODO: Add support for other formats (CSV, XML, etc.)
        return Ok(pcs);
    }
}
