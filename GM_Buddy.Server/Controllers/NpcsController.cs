using GM_Buddy.Contracts.DTOs;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NpcsController : Controller
    {
        private readonly ILogger<NpcController> _logger;
        private readonly INpcLogic _npcLogic;

        public NpcsController(ILogger<NpcController> logger, INpcLogic npcLogic)
        {
            _logger = logger;
            _npcLogic = npcLogic;
        }

       // [HttpGet]
       //// [Authorize]
       // public IEnumerable<NpcDto> Npcs(int account_id)
       // {
       //     var allNpcs = _npcLogic.GetNpcList(account_id);
       //     return allNpcs;
       // }
    }
}
