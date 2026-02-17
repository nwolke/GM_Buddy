using GM_Buddy.Business.Mappers;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Models.Pcs;

namespace GM_Buddy.Business.UnitTests;

public class PcMapperTests
{
    [Fact]
    public void MapToPcDto_DoesNotExposeAccountId()
    {
        // Arrange
        var pc = new Pc
        {
            pc_id = 1,
            account_id = 42, // Sensitive data that should NOT be exposed
            name = "Aragorn",
            description = "A ranger from the North",
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        // Act
        var dto = PcMapper.MapToPcDto(pc);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(pc.pc_id, dto.PcId);
        Assert.Equal(pc.name, dto.Name);
        Assert.Equal(pc.description, dto.Description);
        Assert.Equal(pc.created_at, dto.CreatedAt);
        Assert.Equal(pc.updated_at, dto.UpdatedAt);
        
        // Verify that the DTO type does NOT have an account_id property
        // This is a compile-time check, but we'll verify at runtime too
        var properties = typeof(PcDto).GetProperties();
        Assert.DoesNotContain(properties, p => 
            p.Name.Equals("AccountId", StringComparison.OrdinalIgnoreCase) ||
            p.Name.Equals("account_id", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void MapToEntity_FromCreateRequest_SetsCorrectValues()
    {
        // Arrange
        var request = new CreatePcRequest
        {
            Name = "Legolas",
            Description = "An Elven archer"
        };
        var accountId = 123;

        // Act
        var entity = PcMapper.MapToEntity(request, accountId);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(request.Name, entity.name);
        Assert.Equal(request.Description, entity.description);
        Assert.Equal(accountId, entity.account_id);
    }

    [Fact]
    public void MapToEntity_FromUpdateRequest_UpdatesExistingEntity()
    {
        // Arrange
        var existingPc = new Pc
        {
            pc_id = 1,
            account_id = 42,
            name = "Old Name",
            description = "Old description",
            created_at = DateTime.UtcNow.AddDays(-1),
            updated_at = DateTime.UtcNow.AddDays(-1)
        };

        var request = new UpdatePcRequest
        {
            Name = "Updated Name",
            Description = "Updated description"
        };

        // Act
        PcMapper.MapToEntity(request, existingPc);

        // Assert
        Assert.Equal(request.Name, existingPc.name);
        Assert.Equal(request.Description, existingPc.description);
        // Verify that account_id is NOT changed
        Assert.Equal(42, existingPc.account_id);
    }
}
