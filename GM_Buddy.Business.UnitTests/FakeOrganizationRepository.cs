using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;

namespace GM_Buddy.Business.UnitTests;

/// <summary>
/// Fake in-memory implementation of IOrganizationRepository for testing
/// </summary>
internal class FakeOrganizationRepository : IOrganizationRepository
{
    private readonly List<Organization> _organizations;
    private int _nextId = 1;

    public FakeOrganizationRepository(IEnumerable<Organization>? organizations = null)
    {
        _organizations = organizations?.ToList() ?? new List<Organization>();
        if (_organizations.Any())
        {
            _nextId = _organizations.Max(o => o.organization_id) + 1;
        }
    }

    public Task<IEnumerable<Organization>> GetOrganizationsByAccountIdAsync(int accountId, CancellationToken ct = default)
    {
        var result = _organizations.Where(o => o.account_id == accountId);
        return Task.FromResult(result.AsEnumerable());
    }

    public Task<Organization?> GetOrganizationByIdAsync(int organizationId, CancellationToken ct = default)
    {
        var org = _organizations.FirstOrDefault(o => o.organization_id == organizationId);
        return Task.FromResult(org);
    }

    public Task<IEnumerable<Organization>> GetOrganizationsByCampaignIdAsync(int campaignId, CancellationToken ct = default)
    {
        // In real implementation, this would query entity_relationship table
        // For testing, just return empty
        return Task.FromResult(Enumerable.Empty<Organization>());
    }

    public Task<IEnumerable<Organization>> SearchOrganizationsAsync(int accountId, string searchTerm, CancellationToken ct = default)
    {
        var result = _organizations.Where(o => 
            o.account_id == accountId && 
            (o.name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
             (o.description != null && o.description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))));
        return Task.FromResult(result.AsEnumerable());
    }

    public Task<int> CreateOrganizationAsync(Organization organization, CancellationToken ct = default)
    {
        organization.organization_id = _nextId++;
        organization.created_at = DateTime.UtcNow;
        organization.updated_at = DateTime.UtcNow;
        _organizations.Add(organization);
        return Task.FromResult(organization.organization_id);
    }

    public Task UpdateOrganizationAsync(Organization organization, CancellationToken ct = default)
    {
        var existing = _organizations.FirstOrDefault(o => o.organization_id == organization.organization_id);
        if (existing != null)
        {
            existing.name = organization.name;
            existing.description = organization.description;
            existing.updated_at = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task DeleteOrganizationAsync(int organizationId, CancellationToken ct = default)
    {
        var org = _organizations.FirstOrDefault(o => o.organization_id == organizationId);
        if (org != null)
        {
            _organizations.Remove(org);
        }
        return Task.CompletedTask;
    }

    public Task<bool> OrganizationExistsAsync(int organizationId, CancellationToken ct = default)
    {
        var exists = _organizations.Any(o => o.organization_id == organizationId);
        return Task.FromResult(exists);
    }
}
