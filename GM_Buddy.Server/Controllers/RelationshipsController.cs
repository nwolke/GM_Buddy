using GM_Buddy.Contracts.Constants;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RelationshipsController : ControllerBase
{
    private readonly ILogger<RelationshipsController> _logger;
    private readonly IRelationshipRepository _repository;
    private readonly IAuthHelper _authHelper;
    private readonly ICampaignLogic _campaignLogic;

    public RelationshipsController(
        ILogger<RelationshipsController> logger,
        IRelationshipRepository repository,
        IAuthHelper authHelper,
        ICampaignLogic campaignLogic)
    {
        _logger = logger;
        _repository = repository;
        _authHelper = authHelper;
        _campaignLogic = campaignLogic;
    }

    #region Relationship Types (reference data — no auth required)

    [AllowAnonymous]
    [HttpGet("types")]
    public async Task<ActionResult<IEnumerable<RelationshipType>>> GetRelationshipTypes()
    {
        IEnumerable<RelationshipType> types = await _repository.GetAllRelationshipTypesAsync();
        return Ok(types);
    }

    [AllowAnonymous]
    [HttpGet("types/{id}")]
    public async Task<ActionResult<RelationshipType>> GetRelationshipType(int id)
    {
        RelationshipType? type = await _repository.GetRelationshipTypeByIdAsync(id);
        if (type == null)
        {
            return NotFound($"Relationship type with ID {id} not found");
        }
        return Ok(type);
    }

    #endregion

    #region Entity Relationships

    [HttpPost]
    public async Task<ActionResult<int>> CreateRelationship([FromBody] EntityRelationship relationship)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        if (!EntityTypes.IsValid(relationship.source_entity_type) || !EntityTypes.IsValid(relationship.target_entity_type))
        {
            return BadRequest($"Invalid entity type. Must be one of: {string.Join(", ", EntityTypes.All)}");
        }

        if (relationship.attitude_score < -5 || relationship.attitude_score > 5)
        {
            return BadRequest("attitude_score must be between -5 and 5");
        }

        // Require campaign_id — account scoping relies on campaign.account_id,
        // so relationships without a campaign would be orphaned/invisible
        if (!relationship.campaign_id.HasValue)
        {
            return BadRequest("campaign_id is required");
        }

        // Verify campaign ownership
        var campaign = await _campaignLogic.GetCampaignAsync(relationship.campaign_id.Value, accountId);
        if (campaign == null)
        {
            return NotFound("Campaign not found");
        }

        bool exists = await _repository.RelationshipExistsAsync(
            relationship.source_entity_type,
            relationship.source_entity_id,
            relationship.target_entity_type,
            relationship.target_entity_id,
            relationship.relationship_type_id,
            relationship.campaign_id.Value
        );

        if (exists)
        {
            return Conflict("A relationship between these entities already exists in this campaign (including inactive relationships). If an inactive relationship exists, consider reactivating it instead of creating a new one.");
        }

        int relationshipId = await _repository.CreateRelationshipAsync(relationship);
        _logger.LogInformation("Created relationship {RelationshipId} for account {AccountId}", relationshipId, accountId);

        return CreatedAtAction(nameof(GetRelationship), new { id = relationshipId }, relationshipId);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EntityRelationship>> GetRelationship(int id)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        EntityRelationship? relationship = await _repository.GetRelationshipByIdAsync(id, accountId);
        if (relationship == null)
        {
            return NotFound($"Relationship with ID {id} not found");
        }
        return Ok(relationship);
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<EntityRelationship>>> GetEntityRelationships(
        string entityType,
        int entityId,
        [FromQuery] bool includeInactive = false)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        if (!EntityTypes.IsValid(entityType))
        {
            return BadRequest($"Invalid entity type. Must be one of: {string.Join(", ", EntityTypes.All)}");
        }

        IEnumerable<EntityRelationship> relationships =
            await _repository.GetRelationshipsForEntityAsync(entityType, entityId, accountId, includeInactive);

        _logger.LogInformation("Retrieved {Count} relationships for {EntityType} {EntityId} (account {AccountId})",
            relationships.Count(), entityType, entityId, accountId);

        return Ok(relationships);
    }

    [HttpGet("from/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<EntityRelationship>>> GetRelationshipsFrom(
        string entityType,
        int entityId,
        [FromQuery] bool includeInactive = false)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        if (!EntityTypes.IsValid(entityType))
        {
            return BadRequest($"Invalid entity type. Must be one of: {string.Join(", ", EntityTypes.All)}");
        }

        IEnumerable<EntityRelationship> relationships =
            await _repository.GetRelationshipsFromEntityAsync(entityType, entityId, accountId, includeInactive);

        return Ok(relationships);
    }

    [HttpGet("to/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<EntityRelationship>>> GetRelationshipsTo(
        string entityType,
        int entityId,
        [FromQuery] bool includeInactive = false)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        if (!EntityTypes.IsValid(entityType))
        {
            return BadRequest($"Invalid entity type. Must be one of: {string.Join(", ", EntityTypes.All)}");
        }

        IEnumerable<EntityRelationship> relationships =
            await _repository.GetRelationshipsToEntityAsync(entityType, entityId, accountId, includeInactive);

        return Ok(relationships);
    }

    [HttpGet("entity/{entityType}/{entityId}/type/{relationshipTypeId}")]
    public async Task<ActionResult<IEnumerable<EntityRelationship>>> GetRelationshipsByType(
        string entityType,
        int entityId,
        int relationshipTypeId,
        [FromQuery] bool includeInactive = false)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        if (!EntityTypes.IsValid(entityType))
        {
            return BadRequest($"Invalid entity type. Must be one of: {string.Join(", ", EntityTypes.All)}");
        }

        IEnumerable<EntityRelationship> relationships =
            await _repository.GetRelationshipsByTypeAsync(entityType, entityId, relationshipTypeId, accountId, includeInactive);

        return Ok(relationships);
    }

    [HttpGet("campaign/{campaignId}")]
    public async Task<ActionResult<IEnumerable<EntityRelationship>>> GetCampaignRelationships(
        int campaignId,
        [FromQuery] bool includeInactive = false)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        // Verify campaign ownership — return 404 for unowned/non-existent campaigns
        var campaign = await _campaignLogic.GetCampaignAsync(campaignId, accountId);
        if (campaign == null)
        {
            return NotFound("Campaign not found");
        }

        IEnumerable<EntityRelationship> relationships =
            await _repository.GetRelationshipsByCampaignAsync(campaignId, accountId, includeInactive);

        _logger.LogInformation("Retrieved {Count} relationships for campaign {CampaignId} (account {AccountId})",
            relationships.Count(), campaignId, accountId);

        return Ok(relationships);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRelationship(int id, [FromBody] EntityRelationship relationship)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        if (id != relationship.entity_relationship_id)
        {
            return BadRequest("Relationship ID mismatch");
        }

        if (relationship.attitude_score < -5 || relationship.attitude_score > 5)
        {
            return BadRequest("attitude_score must be between -5 and 5");
        }

        // Account-scoped fetch — returns null if not owned by this account
        EntityRelationship? existing = await _repository.GetRelationshipByIdAsync(id, accountId);
        if (existing == null)
        {
            return NotFound($"Relationship with ID {id} not found");
        }

        // Lock identifying fields to existing values — prevents retargeting
        // relationships to different entities or campaigns via update payload
        relationship.campaign_id = existing.campaign_id;
        relationship.source_entity_type = existing.source_entity_type;
        relationship.source_entity_id = existing.source_entity_id;
        relationship.target_entity_type = existing.target_entity_type;
        relationship.target_entity_id = existing.target_entity_id;
        relationship.relationship_type_id = existing.relationship_type_id;

        await _repository.UpdateRelationshipAsync(relationship);
        _logger.LogInformation("Updated relationship {RelationshipId} for account {AccountId}", id, accountId);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRelationship(int id)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        // Account-scoped fetch — returns null if not owned by this account
        EntityRelationship? existing = await _repository.GetRelationshipByIdAsync(id, accountId);
        if (existing == null)
        {
            return NotFound($"Relationship with ID {id} not found");
        }

        await _repository.DeleteRelationshipAsync(id);
        _logger.LogInformation("Deleted relationship {RelationshipId} for account {AccountId}", id, accountId);

        return NoContent();
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateRelationship(int id)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        // Account-scoped fetch — returns null if not owned by this account
        EntityRelationship? existing = await _repository.GetRelationshipByIdAsync(id, accountId);
        if (existing == null)
        {
            return NotFound($"Relationship with ID {id} not found");
        }

        await _repository.DeactivateRelationshipAsync(id);
        _logger.LogInformation("Deactivated relationship {RelationshipId} for account {AccountId}", id, accountId);

        return NoContent();
    }

    [HttpPost("{id}/reactivate")]
    public async Task<IActionResult> ReactivateRelationship(int id)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        // Account-scoped fetch — returns null if not owned by this account
        EntityRelationship? existing = await _repository.GetRelationshipByIdAsync(id, accountId);
        if (existing == null)
        {
            return NotFound($"Relationship with ID {id} not found");
        }

        await _repository.ReactivateRelationshipAsync(id);
        _logger.LogInformation("Reactivated relationship {RelationshipId} for account {AccountId}", id, accountId);

        return NoContent();
    }

    [HttpGet("npc/{npcId}/pc-stances")]
    public async Task<ActionResult<IEnumerable<EntityRelationship>>> GetPcStancesForNpc(
        int npcId,
        [FromQuery] int? campaignId = null)
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();

        IEnumerable<EntityRelationship> stances =
            await _repository.GetPcStancesForNpcAsync(npcId, accountId, campaignId);

        return Ok(stances);
    }

    [HttpGet("account")]
    public async Task<IActionResult> GetAllRelationshipsByAccountId()
    {
        int accountId = await _authHelper.GetAuthenticatedAccountIdAsync();
        _logger.LogInformation("Getting all relationships for account {AccountId}", accountId);
        IEnumerable<EntityRelationship> result = await _repository.GetAllRelationshipsOfAccountAsync(accountId);

        _logger.LogInformation("Retrieved {Count} relationships", result.Count());
        return Ok(result);
    }

    #endregion
}
