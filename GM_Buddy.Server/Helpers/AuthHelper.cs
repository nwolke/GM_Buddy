using GM_Buddy.Contracts.Interfaces;
using System.Security.Claims;

namespace GM_Buddy.Server.Helpers;

public class AuthHelper : IAuthHelper
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AuthHelper> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthHelper(
        IAccountRepository accountRepository,
        ILogger<AuthHelper> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _accountRepository = accountRepository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Helper method to get the authenticated user's account ID from JWT claims
    /// </summary>
    public async Task<int> GetAuthenticatedAccountIdAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var cognitoSub = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(cognitoSub))
        {
            _logger.LogWarning("No user identifier found in claims");
            throw new UnauthorizedAccessException("User authentication failed");
        }

        var account = await _accountRepository.GetByCognitoSubAsync(cognitoSub);
        if (account == null)
        {
            _logger.LogWarning("Account not found for cognitoSub: {CognitoSub}", cognitoSub);
            throw new InvalidOperationException("Account not found. Please sync account first.");
        }

        return account.account_id;
    }
}
