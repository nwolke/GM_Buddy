using GM_Buddy.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM_Buddy.Business.Builders;
public abstract class BaseNpcBuilder
{
    protected BaseNpc npc;

    public BaseNpcBuilder CreateBaseNpc()
    {
        npc = new BaseNpc();
        return this;
    }
}
