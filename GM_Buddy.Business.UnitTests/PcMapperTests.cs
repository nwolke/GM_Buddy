using GM_Buddy.Business.Mappers;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Models.Pcs;

namespace GM_Buddy.Business.UnitTests;

public class PcMapperTests
{
    [Fact]
    public void MapToPcDto_MapsAllFields_Correctly()
    {
        // Arrange
        var pc = new Pc
        {
            pc_id = 42,
            account_id = 7,
            name = "Thorin Ironforge",
            description = "A stout dwarven cleric",
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        // Act
        PcDto dto = PcMapper.MapToPcDto(pc);

        // Assert
        Assert.Equal(42, dto.Pc_Id);
        Assert.Equal("Thorin Ironforge", dto.Name);
        Assert.Equal("A stout dwarven cleric", dto.Description);
    }

    [Fact]
    public void MapToPcDto_WithNullDescription_MapsToNull()
    {
        // Arrange
        var pc = new Pc
        {
            pc_id = 1,
            account_id = 1,
            name = "Silent Scout",
            description = null,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        // Act
        PcDto dto = PcMapper.MapToPcDto(pc);

        // Assert
        Assert.Null(dto.Description);
    }

    [Fact]
    public void MapToPcDto_DoesNotIncludeAccountId_InReturnType()
    {
        // Arrange & Act — verify at the type level that PcDto has no account_id property
        var dtoType = typeof(PcDto);

        // Assert
        Assert.Null(dtoType.GetProperty("account_id"));
        Assert.Null(dtoType.GetProperty("Account_Id"));
        Assert.Null(dtoType.GetProperty("AccountId"));
    }

    [Fact]
    public void MapToPcDto_DoesNotIncludeTimestamps_InReturnType()
    {
        // Verify PcDto has no timestamp properties — they're intentionally excluded
        var dtoType = typeof(PcDto);

        Assert.Null(dtoType.GetProperty("Created_At"));
        Assert.Null(dtoType.GetProperty("Updated_At"));
        Assert.Null(dtoType.GetProperty("created_at"));
        Assert.Null(dtoType.GetProperty("updated_at"));
    }
}
