using GM_Buddy.Contracts.Interfaces;
using Microsoft.Extensions.Logging;

namespace GM_Buddy.Business;

public class AccountLogic : IAccountLogic
{
    private readonly ILogger<AccountLogic> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly INpcRepository _npcRepository;
    private readonly IPcRepository _pcRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IRelationshipRepository _relationshipRepository;

    public AccountLogic(
        ILogger<AccountLogic> logger,
        IAccountRepository accountRepository,
        ICampaignRepository campaignRepository,
        INpcRepository npcRepository,
        IPcRepository pcRepository,
        IOrganizationRepository organizationRepository,
        IRelationshipRepository relationshipRepository)
    {
        _logger = logger;
        _accountRepository = accountRepository;
        _campaignRepository = campaignRepository;
        _npcRepository = npcRepository;
        _pcRepository = pcRepository;
        _organizationRepository = organizationRepository;
        _relationshipRepository = relationshipRepository;
    }

    /// <summary>
    /// Deletes an account and all associated application data.
    /// NOTE: This deletes the account from the database but does NOT delete the Cognito user.
    /// The user can log back in with Cognito, which will create a fresh account via the sync endpoint.
    /// This approach allows for "soft" account recovery while still removing all user data from the application.
    /// </summary>
    public async Task DeleteAccountAsync(int accountId)
    {
        _logger.LogWarning("Deleting account {AccountId} and all associated data (Cognito user will remain)", accountId);

        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
        {
            _logger.LogWarning("Account {AccountId} not found for deletion", accountId);
            throw new InvalidOperationException($"Account {accountId} not found");
        }

        await _accountRepository.DeleteAsync(accountId);

        _logger.LogInformation("Account {AccountId} successfully deleted with all cascaded data. Cognito user {CognitoSub} still exists and can recreate account.", 
            accountId, account.cognito_sub);
    }

    /// <summary>
    /// Export all account data as a comprehensive JSON structure.
    /// Includes account info, campaigns, NPCs, PCs, organizations, and relationships.
    /// </summary>
    public async Task<object> ExportAccountDataAsync(int accountId)
    {
        _logger.LogInformation("Exporting all data for account {AccountId}", accountId);

        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
        {
            _logger.LogWarning("Account {AccountId} not found for export", accountId);
            throw new InvalidOperationException($"Account {accountId} not found");
        }

        // Gather all data in parallel for better performance
        var campaignsTask = _campaignRepository.GetByAccountIdAsync(accountId);
        var npcsTask = _npcRepository.GetNpcs(accountId, null); // All NPCs across all campaigns
        var pcsTask = _pcRepository.GetPcsByAccountIdAsync(accountId);
        var organizationsTask = _organizationRepository.GetOrganizationsByAccountIdAsync(accountId);
        var relationshipsTask = _relationshipRepository.GetAllRelationshipsOfAccountAsync(accountId, includeInactive: true);

        await Task.WhenAll(campaignsTask, npcsTask, pcsTask, organizationsTask, relationshipsTask);

        var exportData = new
        {
            ExportMetadata = new
            {
                ExportDate = DateTime.UtcNow,
                AccountId = account.account_id,
                Version = "1.0"
            },
            Account = new
            {
                account.account_id,
                account.email,
                account.username,
                account.first_name,
                account.last_name,
                account.subscription_tier,
                account.created_at,
                account.last_login_at
            },
            Campaigns = campaignsTask.Result,
            NPCs = npcsTask.Result,
            PCs = pcsTask.Result,
            Organizations = organizationsTask.Result,
            Relationships = relationshipsTask.Result
        };

        _logger.LogInformation("Successfully exported data for account {AccountId}: {CampaignCount} campaigns, {NpcCount} NPCs, {PcCount} PCs, {OrgCount} organizations, {RelCount} relationships",
            accountId, 
            campaignsTask.Result.Count(),
            npcsTask.Result.Count(),
            pcsTask.Result.Count(),
            organizationsTask.Result.Count(),
            relationshipsTask.Result.Count());

        return exportData;
    }
}
