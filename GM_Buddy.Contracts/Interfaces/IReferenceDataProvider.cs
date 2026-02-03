namespace GM_Buddy.Contracts.Interfaces;

public interface IReferenceDataProvider
{
    Task<IEnumerable<string>> GetRaceNamesAsync(int gameSystemId, int? accountId = null, CancellationToken ct = default);
    Task<IEnumerable<string>> GetClassNamesAsync(int gameSystemId, int? accountId = null, CancellationToken ct = default);
}
