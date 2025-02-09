using Contracts;
using GM_Buddy.Server.DbContexts;
using GM_Buddy.Server.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;

namespace GM_Buddy.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NpcController : ControllerBase
    {
        private readonly ILogger<NpcController> _logger;
        private readonly GmBuddyDbContext _dbContext;

        public NpcController(ILogger<NpcController> logger, GmBuddyDbContext gmBuddyDbContext)
        {
            _logger = logger;
            _dbContext = gmBuddyDbContext;
        }

        //[Authorize]
        [HttpGet]
        public NpcDto Get(int npc_id)
        {
            var singleNpc = (from npc in _dbContext.Npcs
                             where npc.npc_id == npc_id
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
                             }).FirstOrDefault();
            if (singleNpc != null)
            {
                return singleNpc;
            }
            throw new NullReferenceException();
        }

        //[Authorize]
        [HttpPost]
        public async Task<bool> Post(NpcDto newNpc)
        {
            _dbContext.Npcs.Add(new Npc
            {
                account = _dbContext.Accounts.Where(x => x.account_name == newNpc.account).First(),
                name = newNpc.name,
                description = newNpc.description,
                stats = JsonSerializer.Serialize(newNpc.stats),
                lineage = _dbContext.Lineages.Where(x => x.lineage_name.Equals(newNpc.lineage)).First(),
                occupation = _dbContext.Occupations.Where(x => x.occupation_name.Equals(newNpc.occupation)).First(),
                game_system = _dbContext.GameSystems.Where(x=>x.game_system_name.Equals(newNpc.system)).First(),
            });

            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        [HttpPut]
        public async Task<bool> Put(NpcDto updatedNpc)
        {
            var npcToUpdate = _dbContext.Npcs.FirstOrDefault(x => x.npc_id == updatedNpc.npc_id);
            if (npcToUpdate != null)
            {
                npcToUpdate.description = updatedNpc.description;
                npcToUpdate.name = updatedNpc.name;
                npcToUpdate.stats = JsonSerializer.Serialize(updatedNpc.stats);
                npcToUpdate.lineage = _dbContext.Lineages.First(x => x.lineage_name == updatedNpc.lineage);
                npcToUpdate.occupation = _dbContext.Occupations.First(x => x.occupation_name == updatedNpc.occupation);

                var result = await _dbContext.SaveChangesAsync();
                return result > 0;
            }
            throw new BadHttpRequestException($"Unable to find npc with id={updatedNpc.npc_id}");
        }

        [HttpDelete]
        public async Task<bool> Delete(int npc_id)
        {
            var result = await _dbContext.Npcs.Where(x => x.npc_id == npc_id).ExecuteDeleteAsync();
            return result > 0;
        }
    }

    public class NpcDto
    {
        public int npc_id { get; set; }
        public required string account { get; set; }
        public required string name { get; set; }
        public required string lineage { get; set; }
        public required string occupation { get; set; }
        public required string system { get; set; }
        public DnDStats? stats { get; set; }
        public string? description { get; set; }
    }


}
