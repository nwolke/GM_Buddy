using GM_Buddy.Contracts.DbEntities;

namespace GM_Buddy.Contracts.Interfaces;

public interface IGameSystemRepository
{
    Task<IEnumerable<Game_System>> GetAllAsync(CancellationToken ct = default);
    Task<Game_System?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Game_System?> GetByNameAsync(string name, CancellationToken ct = default);
}
