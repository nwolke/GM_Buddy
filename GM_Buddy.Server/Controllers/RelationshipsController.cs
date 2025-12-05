using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class RelationshipsController : ControllerBase
{
    private readonly ILogger<RelationshipsController> _logger;
    private readonly IRelationshipRepository _repository;

    public RelationshipsController(ILogger<RelationshipsController> logger, IRelationshipRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    #region Relationship Types

    [HttpGet("types")]
    public async Task<ActionResult<IEnumerable<RelationshipType>>> GetRelationshipTypes()
    {
        try
        {
            IEnumerable<RelationshipType> types = await _repository.GetAllRelationshipTypesAsync();
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving relationship types");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("types/{id}")]
    public async Task<ActionResult<RelationshipType>> GetRelationshipType(int id)
    {
        try
        {
            RelationshipType? type = await _repository.GetRelationshipTypeByIdAsync(id);
            if (type == null)
            {
                return NotFound($"Relationship type with ID {id} not found");
            }
            return Ok(type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving relationship type {TypeId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion

    #region Entity Relationships

    [HttpPost]
    public async Task<ActionResult<int>> CreateRelationship([FromBody] EntityRelationship relationship)
    {
        try
        {
            if (!IsValidEntityType(relationship.source_entity_type) || !IsValidEntityType(relationship.target_entity_type))
            {
                return BadRequest("Invalid entity type. Must be 'npc', 'pc', or 'organization'");
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating relationship");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EntityRelationship>> GetRelationship(int id)
    {
        try
        {
            EntityRelationship? relationship = await _repository.GetRelationshipByIdAsync(id);
            if (relationship == null)
            {
                return NotFound($"Relationship with ID {id} not found");
            }
            return Ok(relationship);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving relationship {RelationshipId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<EntityRelationship>>> GetEntityRelationships(
        string entityType, 
        int entityId, 
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            if (!IsValidEntityType(entityType))
            {
                return BadRequest("Invalid entity type. Must be 'npc', 'pc', or 'organization'");
            }

            IEnumerable<EntityRelationship> relationships = 
                await _repository.GetRelationshipsForEntityAsync(entityType, entityId, includeInactive);
            
            _logger.LogInformation("Retrieved {Count} relationships for {EntityType} {EntityId}", 
                relationships.Count(), entityType, entityId);
            
            return Ok(relationships);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving relationships for {EntityType} {EntityId}", entityType, entityId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("from/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<EntityRelationship>>> GetRelationshipsFrom(
        string entityType,
        int entityId,
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            if (!IsValidEntityType(entityType))
            {
                return BadRequest("Invalid entity type. Must be 'npc', 'pc', or 'organization'");
            }

            IEnumerable<EntityRelationship> relationships =
                await _repository.GetRelationshipsFromEntityAsync(entityType, entityId, includeInactive);

            return Ok(relationships);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving relationships from {EntityType} {EntityId}", entityType, entityId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("to/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<EntityRelationship>>> GetRelationshipsTo(
        string entityType,
        int entityId,
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            if (!IsValidEntityType(entityType))
            {
                return BadRequest("Invalid entity type. Must be 'npc', 'pc', or 'organization'");
            }

            IEnumerable<EntityRelationship> relationships =
                await _repository.GetRelationshipsToEntityAsync(entityType, entityId, includeInactive);

            return Ok(relationships);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving relationships to {EntityType} {EntityId}", entityType, entityId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("entity/{entityType}/{entityId}/type/{relationshipTypeId}")]
    public async Task<ActionResult<IEnumerable<EntityRelationship>>> GetRelationshipsByType(
        string entityType,
        int entityId,
        int relationshipTypeId,
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            if (!IsValidEntityType(entityType))
            {
                return BadRequest("Invalid entity type. Must be 'npc', 'pc', or 'organization'");
            }

            IEnumerable<EntityRelationship> relationships =
                await _repository.GetRelationshipsByTypeAsync(entityType, entityId, relationshipTypeId, includeInactive);

            return Ok(relationships);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving {RelationshipTypeId} relationships for {EntityType} {EntityId}", 
                relationshipTypeId, entityType, entityId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("campaign/{campaignId}")]
    public async Task<ActionResult<IEnumerable<EntityRelationship>>> GetCampaignRelationships(
        int campaignId,
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            IEnumerable<EntityRelationship> relationships =
                await _repository.GetRelationshipsByCampaignAsync(campaignId, includeInactive);

            _logger.LogInformation("Retrieved {Count} relationships for campaign {CampaignId}",
                relationships.Count(), campaignId);

            return Ok(relationships);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving relationships for campaign {CampaignId}", campaignId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRelationship(int id, [FromBody] EntityRelationship relationship)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating relationship {RelationshipId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRelationship(int id)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting relationship {RelationshipId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateRelationship(int id)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating relationship {RelationshipId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/reactivate")]
    public async Task<IActionResult> ReactivateRelationship(int id)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating relationship {RelationshipId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion

    #region Helper Methods

    private static bool IsValidEntityType(string entityType)
    {
        return entityType is "npc" or "pc" or "organization";
    }

    #endregion
}
