using GM_Buddy.Contracts.Entities;

namespace GM_Buddy.Contracts.DTOs;
public class DndNpcDto : BaseNpc
{
    public DnDStats? Stats { get; set; }

    public static DndNpcDto CreateDnDNpc(npc_type npc)
    {
        return npc == null
            ? new DndNpcDto
            {

            }
            : new DndNpcDto { };
    }
}
