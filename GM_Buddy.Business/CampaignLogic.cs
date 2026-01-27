using GM_Buddy.Business.Helpers;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.Extensions.Logging;

namespace GM_Buddy.Business;

public class CampaignLogic : ICampaignLogic
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly GameSystemHelper _gameSystemHelper;
    private readonly ILogger<CampaignLogic> _logger;

    public CampaignLogic(
        ICampaignRepository campaignRepository,
        IGameSystemRepository gameSystemRepository,
        ILogger<CampaignLogic> logger)
    {
        _campaignRepository = campaignRepository;
        _gameSystemHelper = new GameSystemHelper(gameSystemRepository, logger);
        _logger = logger;
    }

    public async Task<IEnumerable<Campaign>> GetCampaignsByAccountAsync(int accountId, CancellationToken ct = default)
    {
        try
        {
            var campaigns = await _campaignRepository.GetByAccountIdAsync(accountId, ct);
            return campaigns ?? Enumerable.Empty<Campaign>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaigns for account {AccountId}", accountId);
            throw;
        }
    }

    public async Task<Campaign?> GetCampaignAsync(int campaignId, CancellationToken ct = default)
    {
        try
        {
            var campaign = await _campaignRepository.GetByIdAsync(campaignId, ct);
            return campaign;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaign {CampaignId}", campaignId);
            throw;
        }
    }

    public async Task<int> CreateCampaignAsync(
        int accountId, 
        string name, 
        string? description, 
        int gameSystemId, 
        CancellationToken ct = default)
    {
        try
        {
            var campaign = new Campaign
            {
                account_id = accountId,
                game_system_id = gameSystemId,
                name = name,
                description = description
            };

            int campaignId = await _campaignRepository.CreateAsync(campaign, ct);
            _logger.LogInformation("Created campaign {CampaignId} for account {AccountId}", campaignId, accountId);
            return campaignId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign for account {AccountId}", accountId);
            throw;
        }
    }

    public async Task<bool> UpdateCampaignAsync(
        int campaignId, 
        int accountId, 
        string name, 
        string? description, 
        int gameSystemId, 
        CancellationToken ct = default)
    {
        try
        {
            var campaign = new Campaign
            {
                campaign_id = campaignId,
                account_id = accountId,
                game_system_id = gameSystemId,
                name = name,
                description = description
            };

            bool success = await _campaignRepository.UpdateAsync(campaign, ct);
            if (success)
            {
                _logger.LogInformation("Updated campaign {CampaignId}", campaignId);
            }
            else
            {
                _logger.LogWarning("Campaign {CampaignId} not found or not owned by account {AccountId}", campaignId, accountId);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating campaign {CampaignId}", campaignId);
            throw;
        }
    }

    public async Task<bool> DeleteCampaignAsync(int campaignId, CancellationToken ct = default)
    {
        try
        {
            bool success = await _campaignRepository.DeleteAsync(campaignId, ct);
            if (success)
            {
                _logger.LogInformation("Deleted campaign {CampaignId}", campaignId);
            }
            else
            {
                _logger.LogWarning("Campaign {CampaignId} not found", campaignId);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting campaign {CampaignId}", campaignId);
            throw;
        }
    }
}
