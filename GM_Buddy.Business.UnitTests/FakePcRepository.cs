using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;

namespace GM_Buddy.Business.UnitTests;

/// <summary>
/// Fake in-memory implementation of IPcRepository for testing
/// </summary>
internal class FakePcRepository : IPcRepository
{
    private readonly List<Pc> _pcs;
    private int _nextId = 1;

    public FakePcRepository(IEnumerable<Pc>? pcs = null)
    {
        _pcs = pcs?.ToList() ?? new List<Pc>();
        if (_pcs.Any())
        {
            _nextId = _pcs.Max(p => p.pc_id) + 1;
        }
    }

    public Task<IEnumerable<Pc>> GetPcsByAccountIdAsync(int accountId, CancellationToken ct = default)
    {
        var result = _pcs.Where(p => p.account_id == accountId);
        return Task.FromResult(result.AsEnumerable());
    }

    public Task<Pc?> GetPcByIdAsync(int pcId, CancellationToken ct = default)
    {
        var pc = _pcs.FirstOrDefault(p => p.pc_id == pcId);
        return Task.FromResult(pc);
    }

    public Task<IEnumerable<Pc>> GetPcsByGameSystemIdAsync(int gameSystemId, int accountId, CancellationToken ct = default)
    {
        var result = _pcs.Where(p => p.game_system_id == gameSystemId && p.account_id == accountId);
        return Task.FromResult(result.AsEnumerable());
    }

    public Task<IEnumerable<Pc>> GetPcsByCampaignIdAsync(int campaignId, CancellationToken ct = default)
    {
        // In real implementation, this would query entity_relationship table
        // For testing, just return empty
        return Task.FromResult(Enumerable.Empty<Pc>());
    }

    public Task<int> CreatePcAsync(Pc pc, CancellationToken ct = default)
    {
        pc.pc_id = _nextId++;
        pc.created_at = DateTime.UtcNow;
        pc.updated_at = DateTime.UtcNow;
        _pcs.Add(pc);
        return Task.FromResult(pc.pc_id);
    }

    public Task UpdatePcAsync(Pc pc, CancellationToken ct = default)
    {
        var existing = _pcs.FirstOrDefault(p => p.pc_id == pc.pc_id);
        if (existing != null)
        {
            existing.name = pc.name;
            existing.description = pc.description;
            existing.game_system_id = pc.game_system_id;
            existing.updated_at = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task DeletePcAsync(int pcId, CancellationToken ct = default)
    {
        var pc = _pcs.FirstOrDefault(p => p.pc_id == pcId);
        if (pc != null)
        {
            _pcs.Remove(pc);
        }
        return Task.CompletedTask;
    }

    public Task<bool> PcExistsAsync(int pcId, CancellationToken ct = default)
    {
        var exists = _pcs.Any(p => p.pc_id == pcId);
        return Task.FromResult(exists);
    }
}
