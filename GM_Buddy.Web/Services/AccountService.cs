namespace GM_Buddy.Web.Services;

/// <summary>
/// Service for account-related API calls.
/// Handles syncing Cognito users with the backend database.
/// </summary>
public class AccountService
{
    private readonly ApiService _apiService;
    private readonly ILogger<AccountService> _logger;

    public AccountService(ApiService apiService, ILogger<AccountService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    /// <summary>
    /// Sync account with backend after Cognito login.
    /// Creates account if it doesn't exist, updates last login if it does.
    /// </summary>
    public async Task<AccountInfo?> SyncAccountAsync(string cognitoSub, string? email = null, string? displayName = null)
    {
        try
        {
            _logger.LogInformation("Syncing account for {CognitoSub}", cognitoSub);
            
            var request = new SyncAccountRequest
            {
                CognitoSub = cognitoSub,
                Email = email,
                DisplayName = displayName
            };

            var response = await _apiService.PostAsync<SyncAccountRequest, AccountInfo>("/Account/sync", request);
            
            if (response != null)
            {
                _logger.LogInformation("Account synced successfully. AccountId: {AccountId}", response.AccountId);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync account for {CognitoSub}", cognitoSub);
            return null;
        }
    }

    /// <summary>
    /// Get current account info
    /// </summary>
    public async Task<AccountInfo?> GetAccountAsync(string cognitoSub)
    {
        try
        {
            return await _apiService.GetAsync<AccountInfo>($"/Account/me?cognitoSub={Uri.EscapeDataString(cognitoSub)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get account for {CognitoSub}", cognitoSub);
            return null;
        }
    }
}

/// <summary>
/// Request to sync account
/// </summary>
public class SyncAccountRequest
{
    public required string CognitoSub { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
}

/// <summary>
/// Account information from the API
/// </summary>
public class AccountInfo
{
    public int AccountId { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string SubscriptionTier { get; set; } = "free";
    public DateTime CreatedAt { get; set; }
}
