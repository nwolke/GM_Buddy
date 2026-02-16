using GM_Buddy.Business.Mappers;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.DTOs;

namespace GM_Buddy.Business.UnitTests;

public class CampaignMapperTests
{
    [Fact]
    public void CampaignToDbEntity_ValidDto_MapsCorrectly()
    {
        // Arrange
        var accountId = 123;
        var dto = new CampaignDTO
        {
            Campaign_id = 0,
            Name = "Test Campaign",
            Description = "Test Description"
        };

        // Act
        var result = dto.CampaignToDbEntity(accountId);

        // Assert
        Assert.Equal(accountId, result.account_id);
        Assert.Equal(dto.Name, result.name);
        Assert.Equal(dto.Description, result.description);
    }

    [Fact]
    public void CampaignToDbEntity_NullDescription_MapsCorrectly()
    {
        // Arrange
        var accountId = 123;
        var dto = new CampaignDTO
        {
            Campaign_id = 0,
            Name = "Test Campaign",
            Description = null
        };

        // Act
        var result = dto.CampaignToDbEntity(accountId);

        // Assert
        Assert.Equal(accountId, result.account_id);
        Assert.Equal(dto.Name, result.name);
        Assert.Null(result.description);
    }

    [Fact]
    public void DbEntityToDto_ValidEntity_MapsCorrectly()
    {
        // Arrange
        var entity = new Campaign
        {
            campaign_id = 1,
            account_id = 123,
            name = "Test Campaign",
            description = "Test Description",
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        // Act
        var result = entity.DbEntityToDto();

        // Assert
        Assert.Equal(entity.campaign_id, result.Campaign_id);
        Assert.Equal(entity.name, result.Name);
        Assert.Equal(entity.description, result.Description);
    }

    [Fact]
    public void DbEntityToDto_NullableFields_MapsCorrectly()
    {
        // Arrange
        var entity = new Campaign
        {
            campaign_id = 1,
            account_id = 123,
            name = "Test Campaign",
            description = null,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        // Act
        var result = entity.DbEntityToDto();

        // Assert
        Assert.Equal(entity.campaign_id, result.Campaign_id);
        Assert.Equal(entity.name, result.Name);
        Assert.Null(result.Description);
    }

    [Fact]
    public void DbEntityToDto_DoesNotExpose_AccountId()
    {
        // Arrange
        var entity = new Campaign
        {
            campaign_id = 1,
            account_id = 123,
            name = "Test Campaign",
            description = "Test Description",
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        // Act
        var result = entity.DbEntityToDto();

        // Assert - CampaignDTO should not have an account_id property
        var dtoType = result.GetType();
        var accountIdProperty = dtoType.GetProperty("account_id") ?? dtoType.GetProperty("Account_id");
        Assert.Null(accountIdProperty);
    }
}
