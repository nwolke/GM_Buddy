using Contracts;
using GM_Buddy.Server.DbContexts;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GM_Buddy.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NpcsController : Controller
    {
        private readonly ILogger<NpcController> _logger;
        private readonly GmBuddyDbContext _dbContext;

        public NpcsController(ILogger<NpcController> logger, GmBuddyDbContext gmBuddyDbContext)
        {
            _logger = logger;
            _dbContext = gmBuddyDbContext;
        }

        [HttpGet]
        public IEnumerable<NpcDto> Npcs(int account_id)
        {
            var allNpcs = from npc in _dbContext.Npcs
                          where npc.account.account_id == account_id
                    select new NpcDto
                    {
                        npc_id = npc.npc_id,
                        account = npc.account.account_name,
                        name = npc.name,
                        stats = JsonSerializer.Deserialize<DnDStats>(npc.stats ?? "{}", JsonSerializerOptions.Default),
                        description = npc.description,
                        lineage = npc.lineage.lineage_name,
                        occupation = npc.occupation.occupation_name,
                        system = npc.game_system.game_system_name
                    };
            return allNpcs;
        }
    }
}
