using GM_Buddy.Contracts.DbEntities;

namespace GM_Buddy.Business.UnitTests;

/// <summary>
/// Unit tests for PC Repository functionality
/// </summary>
public class PcRepositoryTests
{
    #region GetPcsByAccountIdAsync Tests

    [Fact]
    public async Task GetPcsByAccountId_ReturnsEmptyList_WhenNoPcsExist()
    {
        // Arrange
        var repo = new FakePcRepository();

        // Act
        var result = await repo.GetPcsByAccountIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPcsByAccountId_ReturnsOnlyPcsForSpecifiedAccount()
    {
        // Arrange
        var pcs = new[]
        {
            new Pc { pc_id = 1, account_id = 1, name = "PC1" },
            new Pc { pc_id = 2, account_id = 1, name = "PC2" },
            new Pc { pc_id = 3, account_id = 2, name = "PC3" }
        };
        var repo = new FakePcRepository(pcs);

        // Act
        var result = await repo.GetPcsByAccountIdAsync(1);

        // Assert
        var pcList = result.ToList();
        Assert.Equal(2, pcList.Count);
        Assert.All(pcList, pc => Assert.Equal(1, pc.account_id));
    }

    #endregion

    #region GetPcByIdAsync Tests

    [Fact]
    public async Task GetPcById_ReturnsNull_WhenPcDoesNotExist()
    {
        // Arrange
        var repo = new FakePcRepository();

        // Act
        var result = await repo.GetPcByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPcById_ReturnsPc_WhenPcExists()
    {
        // Arrange
        var pc = new Pc { pc_id = 42, account_id = 1, name = "Test PC" };
        var repo = new FakePcRepository(new[] { pc });

        // Act
        var result = await repo.GetPcByIdAsync(42);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.pc_id);
        Assert.Equal("Test PC", result.name);
    }

    #endregion

    #region CreatePcAsync Tests

    [Fact]
    public async Task CreatePc_AssignsNewId()
    {
        // Arrange
        var repo = new FakePcRepository();
        var pc = new Pc { account_id = 1, name = "New PC" };

        // Act
        var id = await repo.CreatePcAsync(pc);

        // Assert
        Assert.True(id > 0);
        Assert.Equal(id, pc.pc_id);
    }

    [Fact]
    public async Task CreatePc_SetsTimestamps()
    {
        // Arrange
        var repo = new FakePcRepository();
        var pc = new Pc { account_id = 1, name = "New PC" };
        var beforeCreate = DateTime.UtcNow;

        // Act
        await repo.CreatePcAsync(pc);

        // Assert
        Assert.True(pc.created_at >= beforeCreate);
        Assert.True(pc.updated_at >= beforeCreate);
    }

    [Fact]
    public async Task CreatePc_CanBeRetrievedById()
    {
        // Arrange
        var repo = new FakePcRepository();
        var pc = new Pc { account_id = 1, name = "New PC" };

        // Act
        var id = await repo.CreatePcAsync(pc);
        var retrieved = await repo.GetPcByIdAsync(id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("New PC", retrieved.name);
    }

    #endregion

    #region UpdatePcAsync Tests

    [Fact]
    public async Task UpdatePc_ModifiesExistingPc()
    {
        // Arrange
        var pc = new Pc { pc_id = 1, account_id = 1, name = "Original Name" };
        var repo = new FakePcRepository(new[] { pc });
        
        var updated = new Pc 
        { 
            pc_id = 1, 
            account_id = 1, 
            game_system_id = 1, 
            name = "Updated Name",
            description = "New description"
        };

        // Act
        await repo.UpdatePcAsync(updated);
        var result = await repo.GetPcByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.name);
        Assert.Equal("New description", result.description);
    }

    [Fact]
    public async Task UpdatePc_UpdatesTimestamp()
    {
        // Arrange
        var originalTime = DateTime.UtcNow.AddHours(-1);
        var pc = new Pc 
        { 
            pc_id = 1, 
            account_id = 1, 
            game_system_id = 1, 
            name = "PC",
            updated_at = originalTime
        };
        var repo = new FakePcRepository(new[] { pc });

        // Act
        await Task.Delay(10); // Ensure time difference
        await repo.UpdatePcAsync(pc);
        var result = await repo.GetPcByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.updated_at > originalTime);
    }

    #endregion

    #region DeletePcAsync Tests

    [Fact]
    public async Task DeletePc_RemovesPcFromRepository()
    {
        // Arrange
        var pc = new Pc { pc_id = 1, account_id = 1, name = "To Delete" };
        var repo = new FakePcRepository(new[] { pc });

        // Act
        await repo.DeletePcAsync(1);
        var result = await repo.GetPcByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeletePc_DoesNotThrow_WhenPcDoesNotExist()
    {
        // Arrange
        var repo = new FakePcRepository();

        // Act & Assert (should not throw)
        await repo.DeletePcAsync(999);
    }

    #endregion

    #region PcExistsAsync Tests

    [Fact]
    public async Task PcExists_ReturnsTrue_WhenPcExists()
    {
        // Arrange
        var pc = new Pc { pc_id = 1, account_id = 1, name = "PC" };
        var repo = new FakePcRepository(new[] { pc });

        // Act
        var exists = await repo.PcExistsAsync(1);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task PcExists_ReturnsFalse_WhenPcDoesNotExist()
    {
        // Arrange
        var repo = new FakePcRepository();

        // Act
        var exists = await repo.PcExistsAsync(999);

        // Assert
        Assert.False(exists);
    }

    #endregion
}
