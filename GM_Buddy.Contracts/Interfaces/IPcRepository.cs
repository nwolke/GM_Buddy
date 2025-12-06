using GM_Buddy.Contracts.DbEntities;

namespace GM_Buddy.Contracts.Interfaces;

/// <summary>
/// Repository interface for managing Player Characters (PCs)
/// </summary>
public interface IPcRepository
{
    /// <summary>
    /// Get all PCs for a specific account
    /// </summary>
    Task<IEnumerable<Pc>> GetPcsByAccountIdAsync(int accountId, CancellationToken ct = default);

    /// <summary>
    /// Get a specific PC by ID
    /// </summary>
    Task<Pc?> GetPcByIdAsync(int pcId, CancellationToken ct = default);

    /// <summary>
    /// Get all PCs for a specific game system
    /// </summary>
    Task<IEnumerable<Pc>> GetPcsByGameSystemIdAsync(int gameSystemId, int accountId, CancellationToken ct = default);

    /// <summary>
    /// Get all PCs in a specific campaign
    /// </summary>
    Task<IEnumerable<Pc>> GetPcsByCampaignIdAsync(int campaignId, CancellationToken ct = default);

    /// <summary>
    /// Create a new PC
    /// </summary>
    Task<int> CreatePcAsync(Pc pc, CancellationToken ct = default);

    /// <summary>
    /// Update an existing PC
    /// </summary>
    Task UpdatePcAsync(Pc pc, CancellationToken ct = default);

    /// <summary>
    /// Delete a PC
    /// </summary>
    Task DeletePcAsync(int pcId, CancellationToken ct = default);

    /// <summary>
    /// Check if a PC exists
    /// </summary>
    Task<bool> PcExistsAsync(int pcId, CancellationToken ct = default);
}
