using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GM_Buddy.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly IAuthLogic _authLogic;
    private readonly IAccountLogic _accountLogic;

    public AccountController(
        ILogger<AccountController> logger,
        IAccountRepository accountRepository,
        IAuthLogic authLogic,
        IAccountLogic accountLogic)
    {
        _logger = logger;
        _accountRepository = accountRepository;
        _authLogic = authLogic;
        _accountLogic = accountLogic;
    }

    /// <summary>
    /// Sync account from Cognito. Creates if doesn't exist, updates last login if exists.
    /// Called after successful Cognito authentication.
    /// </summary>
    [HttpPost("sync")]
    [Authorize]
    public async Task<ActionResult<AccountResponse>> SyncAccount([FromBody] SyncAccountRequest request)
    {
        // Extract cognitoSub from JWT token (the 'sub' claim) - NEVER trust it from request body
        // .NET JWT middleware maps 'sub' to ClaimTypes.NameIdentifier
        var cognitoSubClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (cognitoSubClaim == null)
        {
            return Unauthorized("Invalid token: sub claim missing");
        }

        string cognitoSub = cognitoSubClaim.Value;

        var account = await _authLogic.GetOrCreateAccountByCognitoSubAsync(cognitoSub, request.Email);

        _logger.LogInformation("Account synced for {CognitoSub}, AccountId: {AccountId}",
            cognitoSub, account.account_id);

        return Ok(new AccountResponse
        {
            AccountId = account.account_id,
            Email = account.email,
            DisplayName = account.DisplayName,
            SubscriptionTier = account.subscription_tier,
            CreatedAt = account.created_at
        });
    }

    /// <summary>
    /// Get current account info by Cognito sub
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AccountResponse>> GetAccount()
    {
        // Extract cognitoSub from JWT token (the 'sub' claim) - NEVER trust it from query parameters
        // .NET JWT middleware maps 'sub' to ClaimTypes.NameIdentifier
        var cognitoSubClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (cognitoSubClaim == null)
        {
            return Unauthorized("Invalid token: sub claim missing");
        }

        string cognitoSub = cognitoSubClaim.Value;

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

    /// <summary>
    /// Delete the current user's account and all associated data.
    /// This will CASCADE delete all campaigns, NPCs, PCs, organizations, and relationships.
    /// NOTE: The Cognito user will remain in AWS User Pool. The user can log back in to create a new account.
    /// </summary>
    [HttpDelete]
    [Authorize]
    public async Task<ActionResult> DeleteAccount()
    {
        var cognitoSubClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (cognitoSubClaim == null)
        {
            return Unauthorized("Invalid token: sub claim missing");
        }

        string cognitoSub = cognitoSubClaim.Value;

        var account = await _accountRepository.GetByCognitoSubAsync(cognitoSub);
        if (account == null)
        {
            return NotFound("Account not found");
        }

        await _accountLogic.DeleteAccountAsync(account.account_id);

        _logger.LogInformation("Account {AccountId} deleted successfully for user {CognitoSub}",
            account.account_id, cognitoSub);

        return Ok(new { message = "Account successfully deleted" });
    }

    /// <summary>
    /// Export all account data as JSON.
    /// Includes campaigns, NPCs, PCs, organizations, and relationships.
    /// </summary>
    [HttpGet("export")]
    [Authorize]
    public async Task<ActionResult> ExportAccountData()
    {
        var cognitoSubClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (cognitoSubClaim == null)
        {
            return Unauthorized("Invalid token: sub claim missing");
        }

        string cognitoSub = cognitoSubClaim.Value;

        var account = await _accountRepository.GetByCognitoSubAsync(cognitoSub);
        if (account == null)
        {
            return NotFound("Account not found");
        }

        var exportData = await _accountLogic.ExportAccountDataAsync(account.account_id);

        _logger.LogInformation("Account data exported successfully for account {AccountId}", account.account_id);

        return Ok(exportData);
    }
}

/// <summary>
/// Request to sync account from Cognito
/// </summary>
public class SyncAccountRequest
{
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
