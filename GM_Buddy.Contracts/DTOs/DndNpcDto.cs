using GM_Buddy.Contracts.DbModels;

namespace GM_Buddy.Contracts.DTOs;
public class DndNpcDto : BaseNpc
{
    public DnDStats? Stats { get; set; }

    public static DndNpcDto CreateDnDNpc(npc_type npc)
    {
        if(npc == null)
        {
            return new DndNpcDto
            {
                
            };
        }
    }
}
