using GM_Buddy.Contracts.DTOs;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class NpcsController : Controller
{
    private readonly ILogger<NpcController> _logger;
    private readonly INpcLogic _logic;

    public NpcsController(ILogger<NpcController> logger, INpcLogic npcLogic)
    {
        _logger = logger;
        _logic = npcLogic;
    }

    [HttpGet]
    // [Authorize]
    public async Task<IEnumerable<NpcDto>> Npcs(int account_id)
    {
        _logger.LogInformation("getting npcs");
        var result = await _logic.GetNpcList(account_id);
        _logger.LogInformation($"retrieved {result.Count()} logs");
        return result;
    }
}
