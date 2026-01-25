using GM_Buddy.Contracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NpcController : ControllerBase
{
    private readonly ILogger<NpcController> _logger;
    private readonly INpcLogic _logic;

    public NpcController(ILogger<NpcController> logger, INpcLogic npcLogic)
    {
        _logger = logger;
        _logic = npcLogic;
    }

    [Authorize]
    [HttpGet]
    public async Task<dynamic?> Get()
    {
        dynamic? result = await _logic.GetNpc(1);
        return result;

    }

    //[Authorize]
    //        [HttpGet]
    //        public async Task<NpcDto> Get(int npc_id)
    //        {            
    //            var singleNpc = await _logic.GetNpc(npc_id);
    //            if (singleNpc == null)
    //            {
    //                throw new BadHttpRequestException($"Unable to find npc with id={npc_id}");
    //            }
    //            return singleNpc;
    //        }

    //        //[Authorize]
    //        [HttpPost]
    //        public async Task<bool> Post(NpcDto newNpc)
    //        {
    //            return await _logic.UpdateNpc(newNpc);
    //        }

    //        [HttpPut]
    //        public async Task<bool> Put(NpcDto updatedNpc)
    //        {
    //            var result = await _logic.UpdateNpc(updatedNpc);
    //            return result;
    //}

    //        [HttpDelete]
    //        public async Task<bool> Delete(int npc_id)
    //        {
    //            var result = await _logic.DeleteNpc(npc_id);
    //            return result;
    //        }
}


