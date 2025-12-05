using GM_Buddy.Contracts.Models.Npcs;
using GM_Buddy.Contracts.Models.Npcs.Dnd;

namespace GM_Buddy.Business.Factories.DndFactory;
public class DndNpcCreator : NpcCreator
{
    public DndNpcCreator()
    {
        // Constructor logic here
    }

    public override BaseNpc CreateNpc()
    {
        var newNpc = new DndNpc
        {
            Name = "New DnD NPC",
            Stats = new DnDStats
            {
                Attributes = new DndAttributes
                {
                    Strength = 10,
                    Dexterity = 10,
                    Constitution = 10,
                    Intelligence = 10,
                    Wisdom = 10,
                    Charisma = 10
                },
                Languages = ["Common"]
            },
        };
        return newNpc;
    }


    public override BaseNpc GenerateRandomNpc()
    {
        throw new NotImplementedException();
    }
}
