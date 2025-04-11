using GM_Buddy.Business.Mappers;
using GM_Buddy.Contracts;
using GM_Buddy.Contracts.DbModels;
using System.Text.Json;

namespace GM_Buddy.Business.UnitTests;

public class MapperTests
{
    [Fact]
    public void TestMapper_npctype_to_dndnpcdto()
    {
        // Arrange
        var npc = new npc_type
        {
            npc_id = 1,
            user_id = 1,
            name = "Test NPC",
            lineage_name = "Human",
            occupation_name = "Warrior",
            game_system_name = "D&D 5E",
            stats = "{\"attributes\": {\"strength\": 10, \"dexterity\": 12, \"constitution\": 14, \"intelligence\": 8, \"wisdom\": 10, \"charisma\": 16}, \"languages\": [\"common\"]}",
            description = "A test NPC",
            gender = "dude"
        };
        // Act
        var result = NpcMapper.MapToNpcDto(npc);
        // Assert
        Assert.Equal(npc.npc_id, result.Npc_Id);
        Assert.Equal(npc.user_id, result.UserId);
        Assert.Equal(npc.name, result.Name);
        Assert.Equal(npc.lineage_name, result.Lineage);
        Assert.Equal(npc.occupation_name, result.Occupation);
        Assert.Equal(npc.game_system_name, result.System);
        Assert.Equal(JsonSerializer.Deserialize<DnDStats>(npc.stats).Attributes.Constitution, result.Stats.Attributes.Constitution);
        Assert.Equal(npc.description, result.Description);

    }
}
