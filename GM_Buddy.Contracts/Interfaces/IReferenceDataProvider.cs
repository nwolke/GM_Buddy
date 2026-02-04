namespace GM_Buddy.Contracts.Interfaces;

public interface IReferenceDataProvider
{
    Task<IEnumerable<string>> GetLineageNamesAsync(int gameSystemId, int? accountId = null, int? campaignId = null, CancellationToken ct = default);
    Task<IEnumerable<string>> GetOccupationNamesAsync(int gameSystemId, int? accountId = null, int? campaignId = null, CancellationToken ct = default);
}
