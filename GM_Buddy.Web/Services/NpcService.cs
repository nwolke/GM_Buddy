using GM_Buddy.Contracts.Models.Npcs;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GM_Buddy.Web.Services;

/// <summary>
/// Service for NPC-related API calls.
/// </summary>
public class NpcService
{
    private readonly ApiService _apiService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<NpcService> _logger;

    public NpcService(
        ApiService apiService, 
        IHttpContextAccessor httpContextAccessor, 
        ILogger<NpcService> logger)
    {
        _apiService = apiService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Fetch all NPCs for the current authenticated user.
    /// Account must already exist (synced on login).
    /// </summary>
    public async Task<List<NpcViewModel>> GetNpcsAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user?.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("User not authenticated, returning empty NPC list");
            return [];
        }

        try
        {
            // No need to pass cognitoSub - server will extract it from JWT token
            var endpoint = "/Npcs";
            
            _logger.LogInformation("Fetching NPCs for authenticated user");
            var npcs = await _apiService.GetAsync<List<NpcViewModel>>(endpoint);
            return npcs ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch NPCs for authenticated user");
            return [];
        }
    }
}

/// <summary>
/// View model for NPC display.
/// Maps to the API response structure.
/// </summary>
public class NpcViewModel
{
    public int Npc_Id { get; set; }
    public string? Name { get; set; }  // Name is at top level, not in Stats
    public string? Description { get; set; }
    public string? System { get; set; }
    public NpcStats? Stats { get; set; }
}

public class NpcStats
{
    public string? Lineage { get; set; }
    public string? Occupation { get; set; }
    public string? Gender { get; set; }
    public NpcAttributes? Attributes { get; set; }
}

public class NpcAttributes
{
    public int? Strength { get; set; }
    public int? Dexterity { get; set; }
    public int? Constitution { get; set; }
    public int? Intelligence { get; set; }
    public int? Wisdom { get; set; }
    public int? Charisma { get; set; }
}
