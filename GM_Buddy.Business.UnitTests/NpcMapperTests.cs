using System.Text.Json;
using GM_Buddy.Business.Mappers;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Models.Npcs.Dnd;

namespace GM_Buddy.Business.UnitTests;

public class NpcMapperTests
{
    [Fact]
    public void MapToNpcDto_WithValidStats_ParsesStats()
    {
        // Arrange
        var statsJson = JsonSerializer.Serialize(new DnDStats
        {
            Lineage = "Goblin",
            Occupation = "Warrior"
        });

        var npc = new Npc
        {
            npc_id = 1,
            account_id = 2,
            game_system_name = "D&D",
            name = "GoblinBoy",
            stats = statsJson,
            game_system_id = 1
        };

        // Act
        var dto = NpcMapper.MapToNpcDto(npc);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(npc.npc_id, dto.Npc_Id);
        Assert.Equal(npc.account_id, dto.Account_Id);
        Assert.NotNull(dto.Stats);
        Assert.Equal("Goblin", dto.Stats.Lineage);
        Assert.Equal("Warrior", dto.Stats.Occupation);
    }

    [Fact]
    public void MapToNpcDto_WithInvalidStats_ReturnsSafeDefault()
    {
        // Arrange
        var npc = new Npc
        {
            npc_id = 3,
            account_id = 4,
            stats = "not a json",
            name = "OrcWarriorGuy",
            game_system_id = 1
        };

        // Act
        var dto = NpcMapper.MapToNpcDto(npc);

        // Assert
        Assert.NotNull(dto);
        Assert.NotNull(dto.Stats);
        // Fallback produces empty DnDStats
        Assert.Null(dto.Stats.Lineage);
        Assert.Null(dto.Stats.Occupation);
    }
}