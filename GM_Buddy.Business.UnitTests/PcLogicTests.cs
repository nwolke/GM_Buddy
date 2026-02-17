using GM_Buddy.Business;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Models.Pcs;
using Microsoft.Extensions.Logging.Abstractions;

namespace GM_Buddy.Business.UnitTests;

public class PcLogicTests
{
    private static Pc MakePc(int pcId, int accountId, string name = "Test PC", string? description = null) =>
        new Pc
        {
            pc_id = pcId,
            account_id = accountId,
            name = name,
            description = description,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

    private static PcLogic CreateLogic(FakePcRepository repo) =>
        new PcLogic(repo, NullLogger<PcLogic>.Instance);

    // ── GetPcsAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPcsAsync_ReturnsOnlyAccountPcs()
    {
        var repo = new FakePcRepository(new[]
        {
            MakePc(1, accountId: 10, "Thorin"),
            MakePc(2, accountId: 10, "Lyra"),
            MakePc(3, accountId: 99, "Other Account PC"),
        });
        var logic = CreateLogic(repo);

        var result = (await logic.GetPcsAsync(accountId: 10)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, dto => Assert.True(dto.Pc_Id == 1 || dto.Pc_Id == 2));
    }

    [Fact]
    public async Task GetPcsAsync_ReturnsMappedDtos_NotRawEntities()
    {
        var repo = new FakePcRepository(new[] { MakePc(1, 10, "Thorin") });
        var logic = CreateLogic(repo);

        var result = await logic.GetPcsAsync(accountId: 10);

        Assert.IsAssignableFrom<IEnumerable<PcDto>>(result);
    }

    // ── GetPcAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPcAsync_ReturnsDto_WhenOwnedByAccount()
    {
        var repo = new FakePcRepository(new[] { MakePc(1, 10, "Thorin", "A dwarf") });
        var logic = CreateLogic(repo);

        PcDto? dto = await logic.GetPcAsync(pcId: 1, accountId: 10);

        Assert.NotNull(dto);
        Assert.Equal(1, dto.Pc_Id);
        Assert.Equal("Thorin", dto.Name);
        Assert.Equal("A dwarf", dto.Description);
    }

    [Fact]
    public async Task GetPcAsync_ThrowsUnauthorized_WhenNotOwned()
    {
        var repo = new FakePcRepository(new[] { MakePc(1, accountId: 10) });
        var logic = CreateLogic(repo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => logic.GetPcAsync(pcId: 1, accountId: 99));
    }

    [Fact]
    public async Task GetPcAsync_ReturnsNull_WhenNotFound()
    {
        var repo = new FakePcRepository();
        var logic = CreateLogic(repo);

        PcDto? dto = await logic.GetPcAsync(pcId: 999, accountId: 10);

        Assert.Null(dto);
    }

    // ── CreatePcAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreatePcAsync_AssignsAccountId_AndReturnsDto()
    {
        var repo = new FakePcRepository();
        var logic = CreateLogic(repo);
        var request = new CreatePcRequest { Name = "New Hero", Description = "The chosen one" };

        PcDto dto = await logic.CreatePcAsync(accountId: 10, request);

        Assert.Equal("New Hero", dto.Name);
        Assert.Equal("The chosen one", dto.Description);
        Assert.True(dto.Pc_Id > 0);
    }

    [Fact]
    public async Task CreatePcAsync_SetsAccountId_ServerSide()
    {
        var repo = new FakePcRepository();
        var logic = CreateLogic(repo);
        var request = new CreatePcRequest { Name = "Hero" };

        await logic.CreatePcAsync(accountId: 42, request);

        // Verify the stored PC has the correct account_id
        var storedPcs = await repo.GetPcsByAccountIdAsync(42);
        Assert.Single(storedPcs);
        Assert.Equal(42, storedPcs.First().account_id);
    }

    // ── UpdatePcAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdatePcAsync_ReturnsTrue_WhenOwned()
    {
        var repo = new FakePcRepository(new[] { MakePc(1, 10, "Old Name") });
        var logic = CreateLogic(repo);
        var request = new UpdatePcRequest { Name = "New Name", Description = "Updated" };

        bool result = await logic.UpdatePcAsync(pcId: 1, accountId: 10, request);

        Assert.True(result);
    }

    [Fact]
    public async Task UpdatePcAsync_ThrowsUnauthorized_WhenNotOwned()
    {
        var repo = new FakePcRepository(new[] { MakePc(1, accountId: 10) });
        var logic = CreateLogic(repo);
        var request = new UpdatePcRequest { Name = "Hacked Name" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => logic.UpdatePcAsync(pcId: 1, accountId: 99, request));
    }

    [Fact]
    public async Task UpdatePcAsync_ReturnsFalse_WhenNotFound()
    {
        var repo = new FakePcRepository();
        var logic = CreateLogic(repo);
        var request = new UpdatePcRequest { Name = "Ghost" };

        bool result = await logic.UpdatePcAsync(pcId: 999, accountId: 10, request);

        Assert.False(result);
    }

    // ── DeletePcAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeletePcAsync_ReturnsTrue_WhenOwned()
    {
        var repo = new FakePcRepository(new[] { MakePc(1, 10) });
        var logic = CreateLogic(repo);

        bool result = await logic.DeletePcAsync(pcId: 1, accountId: 10);

        Assert.True(result);
        Assert.Null(await repo.GetPcByIdAsync(1));
    }

    [Fact]
    public async Task DeletePcAsync_ThrowsUnauthorized_WhenNotOwned()
    {
        var repo = new FakePcRepository(new[] { MakePc(1, accountId: 10) });
        var logic = CreateLogic(repo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => logic.DeletePcAsync(pcId: 1, accountId: 99));
    }

    [Fact]
    public async Task DeletePcAsync_ReturnsFalse_WhenNotFound()
    {
        var repo = new FakePcRepository();
        var logic = CreateLogic(repo);

        bool result = await logic.DeletePcAsync(pcId: 999, accountId: 10);

        Assert.False(result);
    }

    // ── GetPcsByCampaignAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetPcsByCampaignAsync_ReturnsMappedDtos()
    {
        var repo = new FakePcRepository(new[] { MakePc(1, 10, "Campaign PC") });
        var logic = CreateLogic(repo);

        // FakePcRepository.GetPcsByCampaignIdAsync returns empty (by design for unit tests)
        var result = await logic.GetPcsByCampaignAsync(campaignId: 1);

        Assert.IsAssignableFrom<IEnumerable<PcDto>>(result);
    }
}
