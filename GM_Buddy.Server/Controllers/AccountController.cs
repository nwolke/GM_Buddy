using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly IAccountRepository _accountRepository;

    public AccountController(
        ILogger<AccountController> logger,
        IAccountRepository accountRepository)
    {
        _logger = logger;
        _accountRepository = accountRepository;
    }

    /// <summary>
    /// Sync account from Cognito. Creates if doesn't exist, updates last login if exists.
    /// Called after successful Cognito authentication.
    /// </summary>
    [HttpPost("sync")]
    public async Task<ActionResult<AccountResponse>> SyncAccount([FromBody] SyncAccountRequest request)
    {
        if (string.IsNullOrEmpty(request.CognitoSub))
        {
            return BadRequest("CognitoSub is required");
        }

        try
        {
            var account = await _accountRepository.GetOrCreateByCognitoSubAsync(
                request.CognitoSub, 
                request.Email);

            _logger.LogInformation("Account synced for {CognitoSub}, AccountId: {AccountId}", 
                request.CognitoSub, account.account_id);

            return Ok(new AccountResponse
            {
                AccountId = account.account_id,
                Email = account.email,
                DisplayName = account.DisplayName,
                SubscriptionTier = account.subscription_tier,
                CreatedAt = account.created_at
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing account for {CognitoSub}", request.CognitoSub);
            return StatusCode(500, "Failed to sync account");
        }
    }

    /// <summary>
    /// Get current account info by Cognito sub
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<AccountResponse>> GetAccount([FromQuery] string cognitoSub)
    {
        if (string.IsNullOrEmpty(cognitoSub))
        {
            return BadRequest("cognitoSub is required");
        }

        var account = await _accountRepository.GetByCognitoSubAsync(cognitoSub);
        if (account == null)
        {
            return NotFound("Account not found. Please sync account first.");
        }

        return Ok(new AccountResponse
        {
            AccountId = account.account_id,
            Email = account.email,
            DisplayName = account.DisplayName,
            SubscriptionTier = account.subscription_tier,
            CreatedAt = account.created_at
        });
    }
}

/// <summary>
/// Request to sync account from Cognito
/// </summary>
public class SyncAccountRequest
{
    public required string CognitoSub { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
}

/// <summary>
/// Account information response
/// </summary>
public class AccountResponse
{
    public int AccountId { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string SubscriptionTier { get; set; } = "free";
    public DateTime CreatedAt { get; set; }
}
