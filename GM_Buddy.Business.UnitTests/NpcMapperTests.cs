using System;
using System.Text.Json;
using GM_Buddy.Business.Mappers;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Npcs.Dnd;
using Xunit;

namespace GM_Buddy.Business.UnitTests;

public class NpcMapperTests
{
    [Fact]
    public void MapToNpcDto_WithValidStats_ParsesStats()
    {
        // Arrange
        var statsJson = JsonSerializer.Serialize(new DnDStats
        {
            Attributes = new DndAttributes
            {
                Strength = 10,
                Dexterity = 12,
                Constitution = 14,
                Intelligence = 8,
                Wisdom = 10,
                Charisma = 16
            },
            Languages = new[] { "Common" }
        });

        var npc = new Npc
        {
            npc_id = 1,
            user_id = 2,
            name = "Test NPC",
            lineage_name = "Human",
            occupation_name = "Warrior",
            game_system_name = "D&D",
            stats = statsJson,
            description = "desc",
            gender = "male",
            game_system_id = 1,
            lineage_id = 1,
            occupation_id = 1
        };

        // Act
        var dto = NpcMapper.MapToNpcDto(npc);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(npc.npc_id, dto.Npc_Id);
        Assert.Equal(npc.user_id, dto.User_Id);
        Assert.Equal("Test NPC", dto.Name);
        Assert.Equal("Human", dto.Lineage);
        Assert.Equal("Warrior", dto.Occupation);
        Assert.Equal("D&D", dto.System);
        Assert.Equal("desc", dto.Description);
        Assert.Equal("male", dto.Gender);
        Assert.NotNull(dto.Stats);
        Assert.Equal(14, dto.Stats.Attributes.Constitution);
        Assert.Single(dto.Stats.Languages);
        Assert.Equal("Common", dto.Stats.Languages[0]);
    }

    [Fact]
    public void MapToNpcDto_WithInvalidStats_ReturnsSafeDefault()
    {
        // Arrange
        var npc = new Npc
        {
            npc_id = 3,
            user_id = 4,
            name = "Broken Stats NPC",
            stats = "not a json",
            description = "desc",
            gender = "other",
            game_system_id = 1,
            lineage_id = 1,
            occupation_id = 1
        };

        // Act
        var dto = NpcMapper.MapToNpcDto(npc);

        // Assert
        Assert.NotNull(dto);
        Assert.NotNull(dto.Stats);
        // Fallback produces DndAttributes with default ints (0) and empty languages
        Assert.Equal(0, dto.Stats.Attributes.Strength);
        Assert.Empty(dto.Stats.Languages);
    }
}