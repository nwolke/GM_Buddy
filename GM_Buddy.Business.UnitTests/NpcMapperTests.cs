using GM_Buddy.Business.Mappers;
using GM_Buddy.Contracts.DbEntities;

namespace GM_Buddy.Business.UnitTests;

public class NpcMapperTests
{
    [Fact]
    public void MapToNpcDto_WithValidNpc_MapsAllProperties()
    {
        // Arrange
        var npc = new Npc
        {
            npc_id = 1,
            account_id = 2,
            campaign_id = 1,
            name = "GoblinBoy",
            description = "A small goblin warrior",
            lineage = "Goblin",
            @class = "Warrior",
            faction = "Dark Horde",
            notes = "Known for his sneaky tactics"
        };

        // Act
        var dto = NpcMapper.MapToNpcDto(npc);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(npc.npc_id, dto.Npc_Id);
        Assert.Equal(npc.account_id, dto.Account_Id);
        Assert.Equal(npc.campaign_id, dto.Campaign_Id);
        Assert.Equal(npc.name, dto.Name);
        Assert.Equal(npc.description, dto.Description);
        Assert.Equal("Goblin", dto.Lineage);
        Assert.Equal("Warrior", dto.Class);
        Assert.Equal("Dark Horde", dto.Faction);
        Assert.Equal("Known for his sneaky tactics", dto.Notes);
    }

    [Fact]
    public void MapToNpcDto_WithMinimalNpc_HandlesNullableFields()
    {
        // Arrange
        var npc = new Npc
        {
            npc_id = 3,
            account_id = 4,
            campaign_id = 1,
            name = "OrcWarriorGuy",
            description = null,
            lineage = null,
            @class = null,
            faction = null,
            notes = null
        };

        // Act
        var dto = NpcMapper.MapToNpcDto(npc);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(npc.npc_id, dto.Npc_Id);
        Assert.Equal(npc.name, dto.Name);
        Assert.Null(dto.Lineage);
        Assert.Null(dto.Class);
        Assert.Null(dto.Faction);
        Assert.Null(dto.Notes);
    }
}