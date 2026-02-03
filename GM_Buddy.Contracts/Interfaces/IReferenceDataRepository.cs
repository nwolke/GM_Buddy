using GM_Buddy.Contracts.DbEntities;

namespace GM_Buddy.Contracts.Interfaces;

public interface IReferenceDataRepository
{
    // Race methods
    Task<IEnumerable<ReferenceRace>> GetRacesAsync(int gameSystemId, int? accountId = null, CancellationToken ct = default);
    Task<ReferenceRace?> GetRaceByIdAsync(int raceId, CancellationToken ct = default);
    Task<int> CreateRaceAsync(ReferenceRace race, CancellationToken ct = default);
    Task<bool> UpdateRaceAsync(ReferenceRace race, CancellationToken ct = default);
    Task<bool> DeleteRaceAsync(int raceId, int accountId, CancellationToken ct = default);
    
    // Class methods
    Task<IEnumerable<ReferenceClass>> GetClassesAsync(int gameSystemId, int? accountId = null, CancellationToken ct = default);
    Task<ReferenceClass?> GetClassByIdAsync(int classId, CancellationToken ct = default);
    Task<int> CreateClassAsync(ReferenceClass referenceClass, CancellationToken ct = default);
    Task<bool> UpdateClassAsync(ReferenceClass referenceClass, CancellationToken ct = default);
    Task<bool> DeleteClassAsync(int classId, int accountId, CancellationToken ct = default);
}
