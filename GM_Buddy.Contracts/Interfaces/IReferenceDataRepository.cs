using GM_Buddy.Contracts.DbEntities;

namespace GM_Buddy.Contracts.Interfaces;

public interface IReferenceDataRepository
{
    // Lineage methods
    Task<IEnumerable<ReferenceLineage>> GetLineagesAsync(int gameSystemId, int? accountId = null, int? campaignId = null, CancellationToken ct = default);
    Task<ReferenceLineage?> GetLineageByIdAsync(int lineageId, CancellationToken ct = default);
    Task<int> CreateLineageAsync(ReferenceLineage lineage, CancellationToken ct = default);
    Task<bool> UpdateLineageAsync(ReferenceLineage lineage, CancellationToken ct = default);
    Task<bool> DeleteLineageAsync(int lineageId, int accountId, int? campaignId = null, CancellationToken ct = default);
    
    // Occupation methods
    Task<IEnumerable<ReferenceOccupation>> GetOccupationsAsync(int gameSystemId, int? accountId = null, int? campaignId = null, CancellationToken ct = default);
    Task<ReferenceOccupation?> GetOccupationByIdAsync(int occupationId, CancellationToken ct = default);
    Task<int> CreateOccupationAsync(ReferenceOccupation occupation, CancellationToken ct = default);
    Task<bool> UpdateOccupationAsync(ReferenceOccupation occupation, CancellationToken ct = default);
    Task<bool> DeleteOccupationAsync(int occupationId, int accountId, int? campaignId = null, CancellationToken ct = default);
}
