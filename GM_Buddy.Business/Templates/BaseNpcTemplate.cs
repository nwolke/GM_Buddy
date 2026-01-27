using GM_Buddy.Contracts.Models.Npcs;

namespace GM_Buddy.Business.Templates;
public abstract class BaseNpcTemplate
{
    public BaseNpc CreateBlankNpc()
    {
        return new BaseNpc() { Name=""};
    }

    public BaseNpc CreateNpc(int? npc_id, string name, string description, int game_system_id)
    {
        return new BaseNpc
        {
            Npc_Id = npc_id,
            Name = name,
        };
    }
}
