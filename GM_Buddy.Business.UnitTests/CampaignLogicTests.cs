using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.DTOs;
using Microsoft.Extensions.Logging.Abstractions;

namespace GM_Buddy.Business.UnitTests;

public class CampaignLogicTests
{
    private readonly FakeCampaignRepository _fakeRepository;
    private readonly CampaignLogic _campaignLogic;

    public CampaignLogicTests()
    {
        _fakeRepository = new FakeCampaignRepository();
        _campaignLogic = new CampaignLogic(
            _fakeRepository,
            NullLogger<CampaignLogic>.Instance);
    }

    [Fact]
    public async Task GetCampaignsByAccountAsync_ReturnsMappedDtos()
    {
        // Arrange
        var accountId = 123;
        var campaign1 = new Campaign
        {
            campaign_id = 0,
            account_id = accountId,
            name = "Campaign 1",
            description = "Description 1",
            game_system_id = 1,
            game_system_name = "D&D 5E",
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };
        var campaign2 = new Campaign
        {
            campaign_id = 0,
            account_id = accountId,
            name = "Campaign 2",
            description = "Description 2",
            game_system_id = 2,
            game_system_name = "Pathfinder",
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        await _fakeRepository.CreateAsync(campaign1);
        await _fakeRepository.CreateAsync(campaign2);

        // Act
        var result = await _campaignLogic.GetCampaignsByAccountAsync(accountId);

        // Assert
        Assert.NotNull(result);
        var dtoList = result.ToList();
        Assert.Equal(2, dtoList.Count);
        Assert.Equal("Campaign 1", dtoList[0].Name);
        Assert.Equal("Campaign 2", dtoList[1].Name);
    }

    [Fact]
    public async Task GetCampaignAsync_ExistingCampaign_ReturnsMappedDto()
    {
        // Arrange
        var campaign = new Campaign
        {
            campaign_id = 0,
            account_id = 123,
            name = "Test Campaign",
            description = "Test Description",
            game_system_id = 1,
            game_system_name = "D&D 5E",
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        var createdCampaignId = await _fakeRepository.CreateAsync(campaign);

        // Act
        var result = await _campaignLogic.GetCampaignAsync(createdCampaignId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Campaign", result.Name);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(1, result.Game_system_id);
    }

    [Fact]
    public async Task GetCampaignAsync_NonExistingCampaign_ReturnsNull()
    {
        // Arrange
        var campaignId = 999;

        // Act
        var result = await _campaignLogic.GetCampaignAsync(campaignId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateCampaignAsync_ValidDto_CreatesAndReturnsId()
    {
        // Arrange
        var accountId = 123;
        var dto = new CampaignDTO
        {
            Campaign_id = 0,
            Name = "New Campaign",
            Description = "New Description",
            Game_system_id = 1
        };

        // Act
        var result = await _campaignLogic.CreateCampaignAsync(accountId, dto);

        // Assert
        Assert.True(result > 0);
        var created = await _fakeRepository.GetByIdAsync(result);
        Assert.NotNull(created);
        Assert.Equal(dto.Name, created.name);
        Assert.Equal(dto.Description, created.description);
        Assert.Equal(dto.Game_system_id, created.game_system_id);
        Assert.Equal(accountId, created.account_id);
    }

    [Fact]
    public async Task UpdateCampaignAsync_ValidDto_UpdatesAndReturnsTrue()
    {
        // Arrange
        var accountId = 123;
        var existingCampaign = new Campaign
        {
            campaign_id = 0,
            account_id = accountId,
            name = "Original Campaign",
            description = "Original Description",
            game_system_id = 1,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };
        var createdId = await _fakeRepository.CreateAsync(existingCampaign);

        var dto = new CampaignDTO
        {
            Campaign_id = createdId,
            Name = "Updated Campaign",
            Description = "Updated Description",
            Game_system_id = null // Game system ID should not be updated
        };

        // Act
        var result = await _campaignLogic.UpdateCampaignAsync(accountId, dto);

        // Assert
        Assert.True(result);
        var updated = await _fakeRepository.GetByIdAsync(createdId);
        Assert.NotNull(updated);
        Assert.Equal("Updated Campaign", updated.name);
        Assert.Equal("Updated Description", updated.description);
    }

    [Fact]
    public async Task UpdateCampaignAsync_NonExistingCampaign_ReturnsFalse()
    {
        // Arrange
        var accountId = 123;
        var dto = new CampaignDTO
        {
            Campaign_id = 999,
            Name = "Updated Campaign",
            Description = "Updated Description"
        };

        // Act
        var result = await _campaignLogic.UpdateCampaignAsync(accountId, dto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteCampaignAsync_ExistingCampaign_ReturnsTrue()
    {
        // Arrange
        var campaign = new Campaign
        {
            campaign_id = 0,
            account_id = 123,
            name = "Campaign to Delete",
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };
        var createdId = await _fakeRepository.CreateAsync(campaign);

        // Act
        var result = await _campaignLogic.DeleteCampaignAsync(createdId);

        // Assert
        Assert.True(result);
        var deleted = await _fakeRepository.GetByIdAsync(createdId);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteCampaignAsync_NonExistingCampaign_ReturnsFalse()
    {
        // Arrange - repository is empty

        // Act
        var result = await _campaignLogic.DeleteCampaignAsync(999);

        // Assert
        Assert.False(result);
    }
}
