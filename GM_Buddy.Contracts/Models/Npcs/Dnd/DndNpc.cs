using GM_Buddy.Contracts.Models.Npcs;

namespace GM_Buddy.Contracts.Models.Npcs.Dnd;
public class DndNpc : BaseNpc
{
    public required DnDStats Stats { get; set; }
}
