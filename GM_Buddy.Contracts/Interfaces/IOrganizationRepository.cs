using GM_Buddy.Contracts.DbEntities;

namespace GM_Buddy.Contracts.Interfaces;

/// <summary>
/// Repository interface for managing Organizations
/// </summary>
public interface IOrganizationRepository
{
    /// <summary>
    /// Get all organizations for a specific account
    /// </summary>
    Task<IEnumerable<Organization>> GetOrganizationsByAccountIdAsync(int accountId, CancellationToken ct = default);

    /// <summary>
    /// Get a specific organization by ID
    /// </summary>
    Task<Organization?> GetOrganizationByIdAsync(int organizationId, CancellationToken ct = default);

    /// <summary>
    /// Get all organizations in a specific campaign (via relationships)
    /// </summary>
    Task<IEnumerable<Organization>> GetOrganizationsByCampaignIdAsync(int campaignId, CancellationToken ct = default);

    /// <summary>
    /// Search organizations by name
    /// </summary>
    Task<IEnumerable<Organization>> SearchOrganizationsAsync(int accountId, string searchTerm, CancellationToken ct = default);

    /// <summary>
    /// Create a new organization
    /// </summary>
    Task<int> CreateOrganizationAsync(Organization organization, CancellationToken ct = default);

    /// <summary>
    /// Update an existing organization
    /// </summary>
    Task UpdateOrganizationAsync(Organization organization, CancellationToken ct = default);

    /// <summary>
    /// Delete an organization
    /// </summary>
    Task DeleteOrganizationAsync(int organizationId, CancellationToken ct = default);

    /// <summary>
    /// Check if an organization exists
    /// </summary>
    Task<bool> OrganizationExistsAsync(int organizationId, CancellationToken ct = default);
}
