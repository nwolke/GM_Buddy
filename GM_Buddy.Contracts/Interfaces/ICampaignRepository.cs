using GM_Buddy.Contracts.DbEntities;

namespace GM_Buddy.Contracts.Interfaces;

public interface ICampaignRepository
{
    Task<IEnumerable<Campaign>> GetByAccountIdAsync(int accountId, CancellationToken ct = default);
    Task<Campaign?> GetByIdAsync(int campaignId, CancellationToken ct = default);
    Task<Campaign?> GetByIdAndAccountAsync(int campaignId, int accountId, CancellationToken ct = default);
    Task<int> CreateAsync(Campaign campaign, CancellationToken ct = default);
    Task<bool> DeleteAsync(int campaignId, int accountId, CancellationToken ct = default);
    Task<bool> UpdateAsync(Campaign campaign, CancellationToken ct = default);
}
