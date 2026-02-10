using GM_Buddy.Contracts.Constants;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Server.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RelationshipsController : ControllerBase
{
    private readonly ILogger<RelationshipsController> _logger;
    private readonly IRelationshipRepository _repository;
    private readonly IAuthHelper _authHelper;

    public RelationshipsController(ILogger<RelationshipsController> logger, IRelationshipRepository repository, IAuthHelper authHelper)
    {
        _logger = logger;
        _repository = repository;
        _authHelper = authHelper;
    }

    #region Relationship Types

    [HttpGet("types")]
    public async Task<ActionResult<IEnumerable<RelationshipType>>> GetRelationshipTypes()
    {
        IEnumerable<RelationshipType> types = await _repository.GetAllRelationshipTypesAsync();
        return Ok(types);
    }

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
        if (!EntityTypes.IsValid(relationship.source_entity_type) || !EntityTypes.IsValid(relationship.target_entity_type))
        {
            return BadRequest($"Invalid entity type. Must be one of: {string.Join(", ", EntityTypes.All)}");
        }

        bool exists = await _repository.RelationshipExistsAsync(
            relationship.source_entity_type,
            relationship.source_entity_id,
            relationship.target_entity_type,
            relationship.target_entity_id,
            relationship.relationship_type_id
        );

        if (exists)
        {
            return Conflict("This relationship already exists");
        }

        int relationshipId = await _repository.CreateRelationshipAsync(relationship);
        _logger.LogInformation("Created relationship {RelationshipId}", relationshipId);

        return CreatedAtAction(nameof(GetRelationship), new { id = relationshipId }, relationshipId);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EntityRelationship>> GetRelationship(int id)
    {
        EntityRelationship? relationship = await _repository.GetRelationshipByIdAsync(id);
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
        if (!EntityTypes.IsValid(entityType))
        {
            return BadRequest($"Invalid entity type. Must be one of: {string.Join(", ", EntityTypes.All)}");
        }

        IEnumerable<EntityRelationship> relationships =
            await _repository.GetRelationshipsForEntityAsync(entityType, entityId, includeInactive);

        _logger.LogInformation("Retrieved {Count} relationships for {EntityType} {EntityId}",
            relationships.Count(), entityType, entityId);

        return Ok(relationships);
    }

    [HttpGet("from/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<EntityRelationship>>> GetRelationshipsFrom(
        string entityType,
        int entityId,
        [FromQuery] bool includeInactive = false)
    {
        if (!EntityTypes.IsValid(entityType))
        {
            return BadRequest($"Invalid entity type. Must be one of: {string.Join(", ", EntityTypes.All)}");
        }

        IEnumerable<EntityRelationship> relationships =
            await _repository.GetRelationshipsFromEntityAsync(entityType, entityId, includeInactive);

        return Ok(relationships);
    }

    [HttpGet("to/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<EntityRelationship>>> GetRelationshipsTo(
        string entityType,
        int entityId,
        [FromQuery] bool includeInactive = false)
    {
        if (!EntityTypes.IsValid(entityType))
        {
            return BadRequest($"Invalid entity type. Must be one of: {string.Join(", ", EntityTypes.All)}");
        }

        IEnumerable<EntityRelationship> relationships =
            await _repository.GetRelationshipsToEntityAsync(entityType, entityId, includeInactive);

        return Ok(relationships);
    }

    [HttpGet("entity/{entityType}/{entityId}/type/{relationshipTypeId}")]
    public async Task<ActionResult<IEnumerable<EntityRelationship>>> GetRelationshipsByType(
        string entityType,
        int entityId,
        int relationshipTypeId,
        [FromQuery] bool includeInactive = false)
    {
        if (!EntityTypes.IsValid(entityType))
        {
            return BadRequest($"Invalid entity type. Must be one of: {string.Join(", ", EntityTypes.All)}");
        }

        IEnumerable<EntityRelationship> relationships =
            await _repository.GetRelationshipsByTypeAsync(entityType, entityId, relationshipTypeId, includeInactive);

        return Ok(relationships);
    }

    [HttpGet("campaign/{campaignId}")]
    public async Task<ActionResult<IEnumerable<EntityRelationship>>> GetCampaignRelationships(
        int campaignId,
        [FromQuery] bool includeInactive = false)
    {
        IEnumerable<EntityRelationship> relationships =
            await _repository.GetRelationshipsByCampaignAsync(campaignId, includeInactive);

        _logger.LogInformation("Retrieved {Count} relationships for campaign {CampaignId}",
            relationships.Count(), campaignId);

        return Ok(relationships);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRelationship(int id, [FromBody] EntityRelationship relationship)
    {
        if (id != relationship.entity_relationship_id)
        {
            return BadRequest("Relationship ID mismatch");
        }

        EntityRelationship? existing = await _repository.GetRelationshipByIdAsync(id);
        if (existing == null)
        {
            return NotFound($"Relationship with ID {id} not found");
        }

        await _repository.UpdateRelationshipAsync(relationship);
        _logger.LogInformation("Updated relationship {RelationshipId}", id);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRelationship(int id)
    {
        EntityRelationship? existing = await _repository.GetRelationshipByIdAsync(id);
        if (existing == null)
        {
            return NotFound($"Relationship with ID {id} not found");
        }

        await _repository.DeleteRelationshipAsync(id);
        _logger.LogInformation("Deleted relationship {RelationshipId}", id);

        return NoContent();
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateRelationship(int id)
    {
        EntityRelationship? existing = await _repository.GetRelationshipByIdAsync(id);
        if (existing == null)
        {
            return NotFound($"Relationship with ID {id} not found");
        }

        await _repository.DeactivateRelationshipAsync(id);
        _logger.LogInformation("Deactivated relationship {RelationshipId}", id);

        return NoContent();
    }

    [HttpPost("{id}/reactivate")]
    public async Task<IActionResult> ReactivateRelationship(int id)
    {
        EntityRelationship? existing = await _repository.GetRelationshipByIdAsync(id);
        if (existing == null)
        {
            return NotFound($"Relationship with ID {id} not found");
        }

        await _repository.ReactivateRelationshipAsync(id);
        _logger.LogInformation("Reactivated relationship {RelationshipId}", id);

        return NoContent();
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
