using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GM_Buddy.Web.Services;

/// <summary>
/// HTTP client service for making authenticated API calls.
/// Forwards the access token from the current user's session to the API.
/// </summary>
public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ApiService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiService(
        HttpClient httpClient, 
        IHttpContextAccessor httpContextAccessor, 
        ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        
        // Log the BaseAddress for debugging
        _logger.LogInformation("ApiService initialized with BaseAddress: {BaseAddress}", 
            _httpClient.BaseAddress?.ToString() ?? "NULL");
    }

    /// <summary>
    /// Make an authenticated GET request
    /// </summary>
    public async Task<T?> GetAsync<T>(string endpoint)
    {
        _logger.LogDebug("GET request to: {Endpoint}, BaseAddress: {BaseAddress}", 
            endpoint, _httpClient.BaseAddress?.ToString() ?? "NULL");
            
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        await AddAuthHeaderAsync(request);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    /// <summary>
    /// Make an authenticated POST request
    /// </summary>
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(data)
        };
        await AddAuthHeaderAsync(request);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(content, JsonOptions);
    }

    /// <summary>
    /// Make an authenticated DELETE request
    /// </summary>
    public async Task DeleteAsync(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
        await AddAuthHeaderAsync(request);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task AddAuthHeaderAsync(HttpRequestMessage request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            // Get the access token stored by OpenIdConnect
            var accessToken = await context.GetTokenAsync("access_token");
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }
    }
}
