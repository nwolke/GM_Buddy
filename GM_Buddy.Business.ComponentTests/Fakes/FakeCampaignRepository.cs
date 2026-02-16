using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;

namespace GM_Buddy.Business.ComponentTests.Fakes;

internal class FakeCampaignRepository : ICampaignRepository
{
    private readonly Dictionary<int, Campaign> _campaigns = new();
    private int _nextId = 1;

    public void AddCampaign(Campaign campaign)
    {
        if (campaign.campaign_id == 0)
        {
            campaign.campaign_id = _nextId++;
        }
        _campaigns[campaign.campaign_id] = campaign;
    }

    public Task<Campaign?> GetByIdAsync(int campaignId, CancellationToken ct = default)
    {
        _campaigns.TryGetValue(campaignId, out var campaign);
        return Task.FromResult(campaign);
    }

    public Task<Campaign?> GetByIdAndAccountAsync(int campaignId, int accountId, CancellationToken ct = default)
    {
        _campaigns.TryGetValue(campaignId, out var campaign);
        if (campaign != null && campaign.account_id != accountId)
        {
            return Task.FromResult<Campaign?>(null);
        }
        return Task.FromResult(campaign);
    }

    public Task<IEnumerable<Campaign>> GetByAccountIdAsync(int accountId, CancellationToken ct = default)
    {
        var campaigns = _campaigns.Values.Where(c => c.account_id == accountId);
        return Task.FromResult(campaigns);
    }

    public Task<int> CreateAsync(Campaign campaign, CancellationToken ct = default)
    {
        campaign.campaign_id = _nextId++;
        _campaigns[campaign.campaign_id] = campaign;
        return Task.FromResult(campaign.campaign_id);
    }

    public Task<bool> UpdateAsync(Campaign campaign, CancellationToken ct = default)
    {
        if (!_campaigns.ContainsKey(campaign.campaign_id))
        {
            return Task.FromResult(false);
        }
        _campaigns[campaign.campaign_id] = campaign;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int campaignId, int accountId, CancellationToken ct = default)
    {
        if (_campaigns.TryGetValue(campaignId, out var campaign) && campaign.account_id == accountId)
        {
            return Task.FromResult(_campaigns.Remove(campaignId));
        }
        return Task.FromResult(false);
    }
}
