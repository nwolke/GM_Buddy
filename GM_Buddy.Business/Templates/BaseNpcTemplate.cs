using GM_Buddy.Contracts.Npcs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM_Buddy.Business.Templates;
public abstract class BaseNpcTemplate
{
    public BaseNpc CreateBlankNpc()
    {
        return new BaseNpc();
    }

    public BaseNpc CreateNpc(int? npc_id, string name, string description, int game_system_id)
    {
        return new BaseNpc
        {
            Npc_Id = npc_id,
            Name = name,
            Description = description
        };
    }
}
