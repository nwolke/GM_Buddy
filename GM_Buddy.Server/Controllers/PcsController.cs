using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PcsController : ControllerBase
{
    private readonly ILogger<PcsController> _logger;
    private readonly IPcRepository _repository;

    public PcsController(ILogger<PcsController> logger, IPcRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    /// <summary>
    /// Get all PCs, optionally filtered by account
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pc>>> GetPcs(
        [FromQuery] int? accountId = null)
    {
        if (!accountId.HasValue)
        {
            return BadRequest("Account ID is required");
        }

        IEnumerable<Pc> pcs = await _repository.GetPcsByAccountIdAsync(accountId.Value);

        return Ok(pcs);
    }

    /// <summary>
    /// Get a specific PC by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Pc>> GetPc(int id)
    {
        _logger.LogInformation("Getting PC {PcId}", id);
        Pc? pc = await _repository.GetPcByIdAsync(id);

        if (pc == null)
        {
            return NotFound($"PC with ID {id} not found");
        }

        return Ok(pc);
    }

    /// <summary>
    /// Create a new PC
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> CreatePc([FromBody] Pc pc)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating new PC: {Name}", pc.name);
        int pcId = await _repository.CreatePcAsync(pc);

        return CreatedAtAction(nameof(GetPc), new { id = pcId }, pcId);
    }

    /// <summary>
    /// Update an existing PC
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePc(int id, [FromBody] Pc pc)
    {
        if (id != pc.pc_id)
        {
            return BadRequest("PC ID mismatch");
        }

        if (!await _repository.PcExistsAsync(id))
        {
            return NotFound($"PC with ID {id} not found");
        }

        _logger.LogInformation("Updating PC {PcId}", id);
        await _repository.UpdatePcAsync(pc);

        return NoContent();
    }

    /// <summary>
    /// Delete a PC
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePc(int id)
    {
        if (!await _repository.PcExistsAsync(id))
        {
            return NotFound($"PC with ID {id} not found");
        }

        _logger.LogInformation("Deleting PC {PcId}", id);
        await _repository.DeletePcAsync(id);

        return NoContent();
    }

    /// <summary>
    /// Get all PCs for a specific account
    /// </summary>
    [HttpGet("account/{accountId}")]
    public async Task<ActionResult<IEnumerable<Pc>>> GetPcsByAccount(int accountId)
    {
        _logger.LogInformation("Getting PCs for account {AccountId}", accountId);
        IEnumerable<Pc> pcs = await _repository.GetPcsByAccountIdAsync(accountId);
        return Ok(pcs);
    }

    /// <summary>
    /// Get all PCs in a specific campaign
    /// </summary>
    [HttpGet("campaign/{campaignId}")]
    public async Task<ActionResult<IEnumerable<Pc>>> GetPcsByCampaign(int campaignId)
    {
        _logger.LogInformation("Getting PCs for campaign {CampaignId}", campaignId);
        IEnumerable<Pc> pcs = await _repository.GetPcsByCampaignIdAsync(campaignId);
        return Ok(pcs);
    }

    /// <summary>
    /// Get all active PCs for an account
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Pc>>> GetActivePcs([FromQuery] int accountId)
    {
        _logger.LogInformation("Getting active PCs for account {AccountId}", accountId);
        // For now, return all PCs for the account
        // TODO: Filter to only PCs currently in active campaigns
        IEnumerable<Pc> pcs = await _repository.GetPcsByAccountIdAsync(accountId);
        return Ok(pcs);
    }

    /// <summary>
    /// Get the party (all PCs) for a campaign
    /// </summary>
    [HttpGet("party/{campaignId}")]
    public async Task<ActionResult<IEnumerable<Pc>>> GetParty(int campaignId)
    {
        _logger.LogInformation("Getting party for campaign {CampaignId}", campaignId);
        IEnumerable<Pc> pcs = await _repository.GetPcsByCampaignIdAsync(campaignId);
        return Ok(pcs);
    }

    /// <summary>
    /// Export PCs in specified format
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportPcs(
        [FromQuery] int accountId,
        [FromQuery] string format = "json")
    {
        _logger.LogInformation("Exporting PCs for account {AccountId} in {Format} format",
            accountId, format);

        IEnumerable<Pc> pcs = await _repository.GetPcsByAccountIdAsync(accountId);

        // For now, just return JSON
        // TODO: Add support for other formats (CSV, XML, etc.)
        return Ok(pcs);
    }
}
