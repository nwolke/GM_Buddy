using GM_Buddy.Contracts.DbEntities;

namespace GM_Buddy.Contracts.Interfaces;

public interface ICampaignLogic
{
    Task<IEnumerable<Campaign>> GetCampaignsByAccountAsync(int accountId, CancellationToken ct = default);
    Task<Campaign?> GetCampaignAsync(int campaignId, CancellationToken ct = default);
    Task<int> CreateCampaignAsync(int accountId, string name, string? description, int gameSystemId, CancellationToken ct = default);
    Task<bool> UpdateCampaignAsync(int campaignId, int accountId, string name, string? description, int gameSystemId, CancellationToken ct = default);
    Task<bool> DeleteCampaignAsync(int campaignId, CancellationToken ct = default);
}
