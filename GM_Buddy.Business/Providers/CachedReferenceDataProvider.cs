using GM_Buddy.Contracts.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GM_Buddy.Business.Providers;

public class CachedReferenceDataProvider : IReferenceDataProvider
{
    private readonly IReferenceDataRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedReferenceDataProvider> _logger;
    
    private const int CacheExpirationHours = 24;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(CacheExpirationHours);

    public CachedReferenceDataProvider(
        IReferenceDataRepository repository,
        IMemoryCache cache,
        ILogger<CachedReferenceDataProvider> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<string>> GetLineageNamesAsync(int gameSystemId, int? accountId = null, int? campaignId = null, CancellationToken ct = default)
    {
        // Get SRD lineages from cache
        var cacheKey = $"srd_lineages_{gameSystemId}";
        var srdLineages = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            _logger.LogInformation("Cache miss for {CacheKey}. Loading SRD lineages from database.", cacheKey);
            entry.SetAbsoluteExpiration(CacheExpiration);
            entry.SetPriority(CacheItemPriority.NeverRemove);
            
            var lineages = await _repository.GetLineagesAsync(gameSystemId, accountId: null, campaignId: null, ct);
            return lineages.Select(l => l.name).ToList();
        });

        if (srdLineages == null)
        {
            _logger.LogWarning("SRD lineages cache entry was null for game system {GameSystemId}", gameSystemId);
            srdLineages = new List<string>();
        }
        else
        {
            _logger.LogDebug("Cache hit for {CacheKey}. Returning {Count} SRD lineages.", cacheKey, srdLineages.Count());
        }

        // If no account ID, return only SRD data
        if (accountId == null)
        {
            return srdLineages;
        }

        // Get user/campaign custom lineages (always fresh from DB)
        _logger.LogDebug("Fetching custom lineages for account {AccountId}, campaign {CampaignId}", accountId, campaignId);
        var userLineages = await _repository.GetLineagesAsync(gameSystemId, accountId, campaignId, ct);
        var userLineageNames = userLineages.Where(l => l.account_id != null).Select(l => l.name);

        // Combine SRD and user custom lineages
        var combinedLineages = srdLineages.Concat(userLineageNames).Distinct().OrderBy(n => n);
        _logger.LogDebug("Returning {Count} total lineages (SRD + custom) for account {AccountId}", combinedLineages.Count(), accountId);
        
        return combinedLineages;
    }

    public async Task<IEnumerable<string>> GetOccupationNamesAsync(int gameSystemId, int? accountId = null, int? campaignId = null, CancellationToken ct = default)
    {
        // Get SRD occupations from cache
        var cacheKey = $"srd_occupations_{gameSystemId}";
        var srdOccupations = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            _logger.LogInformation("Cache miss for {CacheKey}. Loading SRD occupations from database.", cacheKey);
            entry.SetAbsoluteExpiration(CacheExpiration);
            entry.SetPriority(CacheItemPriority.NeverRemove);
            
            var occupations = await _repository.GetOccupationsAsync(gameSystemId, accountId: null, campaignId: null, ct);
            return occupations.Select(o => o.name).ToList();
        });

        if (srdOccupations == null)
        {
            _logger.LogWarning("SRD occupations cache entry was null for game system {GameSystemId}", gameSystemId);
            srdOccupations = new List<string>();
        }
        else
        {
            _logger.LogDebug("Cache hit for {CacheKey}. Returning {Count} SRD occupations.", cacheKey, srdOccupations.Count());
        }

        // If no account ID, return only SRD data
        if (accountId == null)
        {
            return srdOccupations;
        }

        // Get user/campaign custom occupations (always fresh from DB)
        _logger.LogDebug("Fetching custom occupations for account {AccountId}, campaign {CampaignId}", accountId, campaignId);
        var userOccupations = await _repository.GetOccupationsAsync(gameSystemId, accountId, campaignId, ct);
        var userOccupationNames = userOccupations.Where(o => o.account_id != null).Select(o => o.name);

        // Combine SRD and user custom occupations
        var combinedOccupations = srdOccupations.Concat(userOccupationNames).Distinct().OrderBy(n => n);
        _logger.LogDebug("Returning {Count} total occupations (SRD + custom) for account {AccountId}", combinedOccupations.Count(), accountId);
        
        return combinedOccupations;
    }
}
