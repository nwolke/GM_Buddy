using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM_Buddy.Business.Factories;
public class NpcFactory
{
    public static NpcCreator CreateNpc(int game_system_id)
    {
        return game_system_id switch
        {
            1 => new DndFactory.DndNpcCreator(),
            _ => throw new NotImplementedException($"Game system '{game_system_id}' is not supported.")
        };
    }
}
