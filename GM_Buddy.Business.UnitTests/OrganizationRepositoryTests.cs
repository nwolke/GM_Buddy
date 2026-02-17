using GM_Buddy.Contracts.DbEntities;

namespace GM_Buddy.Business.UnitTests;

/// <summary>
/// Unit tests for Organization Repository functionality
/// </summary>
public class OrganizationRepositoryTests
{
    #region GetOrganizationsByAccountIdAsync Tests

    [Fact]
    public async Task GetOrganizationsByAccountId_ReturnsEmptyList_WhenNoOrganizationsExist()
    {
        // Arrange
        var repo = new FakeOrganizationRepository();

        // Act
        var result = await repo.GetOrganizationsByAccountIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetOrganizationsByAccountId_ReturnsOnlyOrganizationsForSpecifiedAccount()
    {
        // Arrange
        var orgs = new[]
        {
            new Organization { organization_id = 1, account_id = 1, name = "Org1" },
            new Organization { organization_id = 2, account_id = 1, name = "Org2" },
            new Organization { organization_id = 3, account_id = 2, name = "Org3" }
        };
        var repo = new FakeOrganizationRepository(orgs);

        // Act
        var result = await repo.GetOrganizationsByAccountIdAsync(1);

        // Assert
        var orgList = result.ToList();
        Assert.Equal(2, orgList.Count);
        Assert.All(orgList, org => Assert.Equal(1, org.account_id));
    }

    #endregion

    #region GetOrganizationByIdAsync Tests

    [Fact]
    public async Task GetOrganizationById_ReturnsNull_WhenOrganizationDoesNotExist()
    {
        // Arrange
        var repo = new FakeOrganizationRepository();

        // Act
        var result = await repo.GetOrganizationByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrganizationById_ReturnsOrganization_WhenOrganizationExists()
    {
        // Arrange
        var org = new Organization 
        { 
            organization_id = 42, 
            account_id = 1, 
            name = "Test Organization",
            description = "A test org"
        };
        var repo = new FakeOrganizationRepository(new[] { org });

        // Act
        var result = await repo.GetOrganizationByIdAsync(42);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.organization_id);
        Assert.Equal("Test Organization", result.name);
        Assert.Equal("A test org", result.description);
    }

    #endregion

    #region SearchOrganizationsAsync Tests

    [Fact]
    public async Task SearchOrganizations_FindsByName()
    {
        // Arrange
        var orgs = new[]
        {
            new Organization { organization_id = 1, account_id = 1, name = "Thieves Guild" },
            new Organization { organization_id = 2, account_id = 1, name = "Merchants Guild" },
            new Organization { organization_id = 3, account_id = 1, name = "City Watch" }
        };
        var repo = new FakeOrganizationRepository(orgs);

        // Act
        var result = await repo.SearchOrganizationsAsync(1, "guild");

        // Assert
        var orgList = result.ToList();
        Assert.Equal(2, orgList.Count);
        Assert.All(orgList, org => Assert.Contains("Guild", org.name));
    }

    [Fact]
    public async Task SearchOrganizations_IsCaseInsensitive()
    {
        // Arrange
        var orgs = new[]
        {
            new Organization { organization_id = 1, account_id = 1, name = "UPPER CASE ORG" },
            new Organization { organization_id = 2, account_id = 1, name = "lower case org" }
        };
        var repo = new FakeOrganizationRepository(orgs);

        // Act
        var result = await repo.SearchOrganizationsAsync(1, "OrG");

        // Assert
        var orgList = result.ToList();
        Assert.Equal(2, orgList.Count);
    }

    [Fact]
    public async Task SearchOrganizations_FindsByDescription()
    {
        // Arrange
        var orgs = new[]
        {
            new Organization 
            { 
                organization_id = 1, 
                account_id = 1, 
                name = "Org1",
                description = "A secret society" 
            },
            new Organization 
            { 
                organization_id = 2, 
                account_id = 1, 
                name = "Org2",
                description = "A public guild" 
            }
        };
        var repo = new FakeOrganizationRepository(orgs);

        // Act
        var result = await repo.SearchOrganizationsAsync(1, "secret");

        // Assert
        var orgList = result.ToList();
        Assert.Single(orgList);
        Assert.Contains("secret", orgList[0].description);
    }

    [Fact]
    public async Task SearchOrganizations_RespectsAccountFilter()
    {
        // Arrange
        var orgs = new[]
        {
            new Organization { organization_id = 1, account_id = 1, name = "Guild" },
            new Organization { organization_id = 2, account_id = 2, name = "Guild" }
        };
        var repo = new FakeOrganizationRepository(orgs);

        // Act
        var result = await repo.SearchOrganizationsAsync(1, "guild");

        // Assert
        var orgList = result.ToList();
        Assert.Single(orgList);
        Assert.Equal(1, orgList[0].account_id);
    }

    #endregion

    #region CreateOrganizationAsync Tests

    [Fact]
    public async Task CreateOrganization_AssignsNewId()
    {
        // Arrange
        var repo = new FakeOrganizationRepository();
        var org = new Organization { account_id = 1, name = "New Org" };

        // Act
        var id = await repo.CreateOrganizationAsync(org);

        // Assert
        Assert.True(id > 0);
        Assert.Equal(id, org.organization_id);
    }

    [Fact]
    public async Task CreateOrganization_SetsTimestamps()
    {
        // Arrange
        var repo = new FakeOrganizationRepository();
        var org = new Organization { account_id = 1, name = "New Org" };
        var beforeCreate = DateTime.UtcNow;

        // Act
        await repo.CreateOrganizationAsync(org);

        // Assert
        Assert.True(org.created_at >= beforeCreate);
        Assert.True(org.updated_at >= beforeCreate);
    }

    [Fact]
    public async Task CreateOrganization_CanBeRetrievedById()
    {
        // Arrange
        var repo = new FakeOrganizationRepository();
        var org = new Organization { account_id = 1, name = "New Org" };

        // Act
        var id = await repo.CreateOrganizationAsync(org);
        var retrieved = await repo.GetOrganizationByIdAsync(id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("New Org", retrieved.name);
    }

    #endregion

    #region UpdateOrganizationAsync Tests

    [Fact]
    public async Task UpdateOrganization_ModifiesExistingOrganization()
    {
        // Arrange
        var org = new Organization 
        { 
            organization_id = 1, 
            account_id = 1, 
            name = "Original Name" 
        };
        var repo = new FakeOrganizationRepository(new[] { org });

        var updated = new Organization 
        { 
            organization_id = 1, 
            account_id = 1, 
            name = "Updated Name",
            description = "New description"
        };

        // Act
        await repo.UpdateOrganizationAsync(updated);
        var result = await repo.GetOrganizationByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.name);
        Assert.Equal("New description", result.description);
    }

    [Fact]
    public async Task UpdateOrganization_UpdatesTimestamp()
    {
        // Arrange
        var originalTime = DateTime.UtcNow.AddHours(-1);
        var org = new Organization 
        { 
            organization_id = 1, 
            account_id = 1, 
            name = "Org",
            updated_at = originalTime
        };
        var repo = new FakeOrganizationRepository(new[] { org });

        // Act
        await Task.Delay(10); // Ensure time difference
        await repo.UpdateOrganizationAsync(org);
        var result = await repo.GetOrganizationByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.updated_at > originalTime);
    }

    #endregion

    #region DeleteOrganizationAsync Tests

    [Fact]
    public async Task DeleteOrganization_RemovesOrganizationFromRepository()
    {
        // Arrange
        var org = new Organization { organization_id = 1, account_id = 1, name = "To Delete" };
        var repo = new FakeOrganizationRepository(new[] { org });

        // Act
        await repo.DeleteOrganizationAsync(1);
        var result = await repo.GetOrganizationByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteOrganization_DoesNotThrow_WhenOrganizationDoesNotExist()
    {
        // Arrange
        var repo = new FakeOrganizationRepository();

        // Act & Assert (should not throw)
        await repo.DeleteOrganizationAsync(999);
    }

    #endregion

    #region OrganizationExistsAsync Tests

    [Fact]
    public async Task OrganizationExists_ReturnsTrue_WhenOrganizationExists()
    {
        // Arrange
        var org = new Organization { organization_id = 1, account_id = 1, name = "Org" };
        var repo = new FakeOrganizationRepository(new[] { org });

        // Act
        var exists = await repo.OrganizationExistsAsync(1);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task OrganizationExists_ReturnsFalse_WhenOrganizationDoesNotExist()
    {
        // Arrange
        var repo = new FakeOrganizationRepository();

        // Act
        var exists = await repo.OrganizationExistsAsync(999);

        // Assert
        Assert.False(exists);
    }

    #endregion
}
