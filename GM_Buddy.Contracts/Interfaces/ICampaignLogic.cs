using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.DTOs;

namespace GM_Buddy.Contracts.Interfaces;

public interface ICampaignLogic
{
    Task<IEnumerable<CampaignDTO>> GetCampaignsByAccountAsync(int accountId, CancellationToken ct = default);
    Task<CampaignDTO?> GetCampaignAsync(int campaignId, CancellationToken ct = default);
    Task<int> CreateCampaignAsync(int accountId, CampaignDTO campaignDto, CancellationToken ct = default);
    Task<bool> UpdateCampaignAsync(int accountId, CampaignDTO campaignDto, CancellationToken ct = default);
    Task<bool> DeleteCampaignAsync(int campaignId, CancellationToken ct = default);
}
