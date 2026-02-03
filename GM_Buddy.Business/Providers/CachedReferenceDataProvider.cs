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

    public async Task<IEnumerable<string>> GetRaceNamesAsync(int gameSystemId, int? accountId = null, CancellationToken ct = default)
    {
        // Get SRD races from cache
        var cacheKey = $"srd_races_{gameSystemId}";
        var srdRaces = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            _logger.LogInformation("Cache miss for {CacheKey}. Loading SRD races from database.", cacheKey);
            entry.SetAbsoluteExpiration(CacheExpiration);
            entry.SetPriority(CacheItemPriority.NeverRemove);
            
            var races = await _repository.GetRacesAsync(gameSystemId, accountId: null, ct);
            return races.Select(r => r.name).ToList();
        });

        if (srdRaces == null)
        {
            _logger.LogWarning("SRD races cache entry was null for game system {GameSystemId}", gameSystemId);
            srdRaces = new List<string>();
        }
        else
        {
            _logger.LogDebug("Cache hit for {CacheKey}. Returning {Count} SRD races.", cacheKey, srdRaces.Count());
        }

        // If no account ID, return only SRD data
        if (accountId == null)
        {
            return srdRaces;
        }

        // Get user custom races (always fresh from DB)
        _logger.LogDebug("Fetching custom races for account {AccountId}", accountId);
        var userRaces = await _repository.GetRacesAsync(gameSystemId, accountId, ct);
        var userRaceNames = userRaces.Where(r => r.account_id != null).Select(r => r.name);

        // Combine SRD and user custom races
        var combinedRaces = srdRaces.Concat(userRaceNames).Distinct().OrderBy(n => n);
        _logger.LogDebug("Returning {Count} total races (SRD + custom) for account {AccountId}", combinedRaces.Count(), accountId);
        
        return combinedRaces;
    }

    public async Task<IEnumerable<string>> GetClassNamesAsync(int gameSystemId, int? accountId = null, CancellationToken ct = default)
    {
        // Get SRD classes from cache
        var cacheKey = $"srd_classes_{gameSystemId}";
        var srdClasses = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            _logger.LogInformation("Cache miss for {CacheKey}. Loading SRD classes from database.", cacheKey);
            entry.SetAbsoluteExpiration(CacheExpiration);
            entry.SetPriority(CacheItemPriority.NeverRemove);
            
            var classes = await _repository.GetClassesAsync(gameSystemId, accountId: null, ct);
            return classes.Select(c => c.name).ToList();
        });

        if (srdClasses == null)
        {
            _logger.LogWarning("SRD classes cache entry was null for game system {GameSystemId}", gameSystemId);
            srdClasses = new List<string>();
        }
        else
        {
            _logger.LogDebug("Cache hit for {CacheKey}. Returning {Count} SRD classes.", cacheKey, srdClasses.Count());
        }

        // If no account ID, return only SRD data
        if (accountId == null)
        {
            return srdClasses;
        }

        // Get user custom classes (always fresh from DB)
        _logger.LogDebug("Fetching custom classes for account {AccountId}", accountId);
        var userClasses = await _repository.GetClassesAsync(gameSystemId, accountId, ct);
        var userClassNames = userClasses.Where(c => c.account_id != null).Select(c => c.name);

        // Combine SRD and user custom classes
        var combinedClasses = srdClasses.Concat(userClassNames).Distinct().OrderBy(n => n);
        _logger.LogDebug("Returning {Count} total classes (SRD + custom) for account {AccountId}", combinedClasses.Count(), accountId);
        
        return combinedClasses;
    }
}
