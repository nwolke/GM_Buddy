using GM_Buddy.Contracts.Npcs;

namespace GM_Buddy.Business.Factories;
public abstract class NpcCreator
{
   public abstract BaseNpc CreateNpc();
    public abstract BaseNpc GenerateRandomNpc();


}
