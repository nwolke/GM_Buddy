using GM_Buddy.Business.Mappers;
using GM_Buddy.Contracts.DTOs;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.Extensions.Logging;

namespace GM_Buddy.Business;

public class CampaignLogic : ICampaignLogic
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ILogger<CampaignLogic> _logger;

    public CampaignLogic(
        ICampaignRepository campaignRepository,
        ILogger<CampaignLogic> logger)
    {
        _campaignRepository = campaignRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<CampaignDTO>> GetCampaignsByAccountAsync(int accountId, CancellationToken ct = default)
    {
        try
        {
            var campaigns = await _campaignRepository.GetByAccountIdAsync(accountId, ct);
            return campaigns?.Select(c => c.DbEntityToDto()) ?? Enumerable.Empty<CampaignDTO>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaigns for account {AccountId}", accountId);
            throw;
        }
    }

    public async Task<CampaignDTO?> GetCampaignAsync(int campaignId, CancellationToken ct = default)
    {
        try
        {
            var campaign = await _campaignRepository.GetByIdAsync(campaignId, ct);
            return campaign?.DbEntityToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaign {CampaignId}", campaignId);
            throw;
        }
    }

    public async Task<CampaignDTO?> GetCampaignAsync(int campaignId, int accountId, CancellationToken ct = default)
    {
        try
        {
            var campaign = await _campaignRepository.GetByIdAndAccountAsync(campaignId, accountId, ct);
            return campaign?.DbEntityToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaign {CampaignId} for account {AccountId}", campaignId, accountId);
            throw;
        }
    }

    public async Task<int> CreateCampaignAsync(
        int accountId, 
        CampaignDTO campaignDto,
        CancellationToken ct = default)
    {
        try
        {
            var campaign = campaignDto.CampaignToDbEntity(accountId);

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
        int accountId, 
        CampaignDTO campaignDto,
        CancellationToken ct = default)
    {
        try
        {
            var campaign = campaignDto.CampaignToDbEntity(accountId);

            bool success = await _campaignRepository.UpdateAsync(campaign, ct);
            if (success)
            {
                _logger.LogInformation("Updated campaign {CampaignId}", campaignDto.Campaign_id);
            }
            else
            {
                _logger.LogWarning("Campaign {CampaignId} not found or not owned by account {AccountId}", campaignDto.Campaign_id, accountId);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating campaign {CampaignId}", campaignDto.Campaign_id);
            throw;
        }
    }

    public async Task<bool> DeleteCampaignAsync(int campaignId, int accountId, CancellationToken ct = default)
    {
        try
        {
            bool success = await _campaignRepository.DeleteAsync(campaignId, accountId, ct);
            if (success)
            {
                _logger.LogInformation("Deleted campaign {CampaignId}", campaignId);
            }
            else
            {
                _logger.LogWarning("Campaign {CampaignId} not found or not owned by account {AccountId}", campaignId, accountId);
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
