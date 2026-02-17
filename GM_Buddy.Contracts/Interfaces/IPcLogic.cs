using GM_Buddy.Contracts.Models.Pcs;

namespace GM_Buddy.Contracts.Interfaces;

public interface IPcLogic
{
    Task<IEnumerable<PcDto>> GetPcsAsync(int accountId, CancellationToken ct = default);
    Task<IEnumerable<PcDto>> GetPcsByCampaignAsync(int campaignId, CancellationToken ct = default);
    Task<PcDto?> GetPcAsync(int pcId, int accountId, CancellationToken ct = default);
    Task<PcDto> CreatePcAsync(int accountId, CreatePcRequest request, CancellationToken ct = default);
    Task<bool> UpdatePcAsync(int pcId, int accountId, UpdatePcRequest request, CancellationToken ct = default);
    Task<bool> DeletePcAsync(int pcId, int accountId, CancellationToken ct = default);
}
