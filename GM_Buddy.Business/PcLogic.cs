using GM_Buddy.Business.Mappers;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Contracts.Models.Pcs;
using Microsoft.Extensions.Logging;

namespace GM_Buddy.Business;

public class PcLogic : IPcLogic
{
    private readonly IPcRepository _pcRepository;
    private readonly ILogger<PcLogic> _logger;

    public PcLogic(IPcRepository pcRepository, ILogger<PcLogic> logger)
    {
        _pcRepository = pcRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<PcDto>> GetPcsAsync(int accountId, CancellationToken ct = default)
    {
        var pcs = await _pcRepository.GetPcsByAccountIdAsync(accountId, ct);
        return pcs.Select(PcMapper.MapToPcDto);
    }

    public async Task<IEnumerable<PcDto>> GetPcsByCampaignAsync(int campaignId, CancellationToken ct = default)
    {
        var pcs = await _pcRepository.GetPcsByCampaignIdAsync(campaignId, ct);
        return pcs.Select(PcMapper.MapToPcDto);
    }

    public async Task<PcDto?> GetPcAsync(int pcId, int accountId, CancellationToken ct = default)
    {
        try
        {
            var pc = await _pcRepository.GetPcByIdAsync(pcId, ct);
            if (pc is null) return null;

            if (pc.account_id != accountId)
            {
                throw new UnauthorizedAccessException(
                    $"Account {accountId} does not own PC {pcId}");
            }

            return PcMapper.MapToPcDto(pc);
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching PC {PcId}", pcId);
            return null;
        }
    }

    public async Task<PcDto> CreatePcAsync(int accountId, CreatePcRequest request, CancellationToken ct = default)
    {
        var pc = new Pc
        {
            account_id = accountId,
            name = request.Name,
            description = request.Description
        };

        int pcId = await _pcRepository.CreatePcAsync(pc, ct);
        _logger.LogInformation("Created PC {PcId} for account {AccountId}", pcId, accountId);

        var created = await _pcRepository.GetPcByIdAsync(pcId, ct);
        return PcMapper.MapToPcDto(created!);
    }

    public async Task<bool> UpdatePcAsync(int pcId, int accountId, UpdatePcRequest request, CancellationToken ct = default)
    {
        try
        {
            var existing = await _pcRepository.GetPcByIdAsync(pcId, ct);
            if (existing is null) return false;

            if (existing.account_id != accountId)
            {
                throw new UnauthorizedAccessException(
                    $"Account {accountId} does not own PC {pcId}");
            }

            existing.name = request.Name;
            existing.description = request.Description;

            await _pcRepository.UpdatePcAsync(existing, ct);
            _logger.LogInformation("Updated PC {PcId} for account {AccountId}", pcId, accountId);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating PC {PcId}", pcId);
            throw;
        }
    }

    public async Task<bool> DeletePcAsync(int pcId, int accountId, CancellationToken ct = default)
    {
        try
        {
            var existing = await _pcRepository.GetPcByIdAsync(pcId, ct);
            if (existing is null) return false;

            if (existing.account_id != accountId)
            {
                throw new UnauthorizedAccessException(
                    $"Account {accountId} does not own PC {pcId}");
            }

            await _pcRepository.DeletePcAsync(pcId, ct);
            _logger.LogInformation("Deleted PC {PcId} for account {AccountId}", pcId, accountId);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting PC {PcId}", pcId);
            throw;
        }
    }
}
