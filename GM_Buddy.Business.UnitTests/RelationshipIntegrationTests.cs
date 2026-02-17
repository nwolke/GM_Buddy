using GM_Buddy.Contracts.Constants;
using GM_Buddy.Contracts.DbEntities;

namespace GM_Buddy.Business.UnitTests;

/// <summary>
/// Integration tests for relationship scenarios across multiple entities
/// </summary>
public class RelationshipIntegrationTests
{
    [Fact]
    public async Task CreateNpcToPcFriendship_WorksEndToEnd()
    {
        // Arrange
        var relationshipRepo = new FakeRelationshipRepository();

        var relationship = new EntityRelationship
        {
            source_entity_type = EntityTypes.Npc,
            source_entity_id = 1,
            target_entity_type = EntityTypes.Pc,
            target_entity_id = 5,
            relationship_type_id = 1, // Friend
            strength = 8,
            description = "Met during the Battle of Silverforge"
        };

        // Act
        var id = await relationshipRepo.CreateRelationshipAsync(relationship);
        var retrieved = await relationshipRepo.GetRelationshipByIdAsync(id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(EntityTypes.Npc, retrieved.source_entity_type);
        Assert.Equal(1, retrieved.source_entity_id);
        Assert.Equal(EntityTypes.Pc, retrieved.target_entity_type);
        Assert.Equal(5, retrieved.target_entity_id);
        Assert.Equal(8, retrieved.strength);
    }

    [Fact]
    public async Task OrganizationMembership_MultipleEntitiesCanJoin()
    {
        // Arrange
        var relationshipRepo = new FakeRelationshipRepository();

        // Create NPC member
        var npcMember = new EntityRelationship
        {
            source_entity_type = EntityTypes.Npc,
            source_entity_id = 1,
            target_entity_type = EntityTypes.Organization,
            target_entity_id = 10,
            relationship_type_id = 7, // Member
            is_active = true
        };

        // Create PC member
        var pcMember = new EntityRelationship
        {
            source_entity_type = EntityTypes.Pc,
            source_entity_id = 2,
            target_entity_type = EntityTypes.Organization,
            target_entity_id = 10,
            relationship_type_id = 7, // Member
            is_active = true
        };

        // Act
        await relationshipRepo.CreateRelationshipAsync(npcMember);
        await relationshipRepo.CreateRelationshipAsync(pcMember);

        var members = await relationshipRepo.GetRelationshipsToEntityAsync(EntityTypes.Organization, 10);

        // Assert
        var memberList = members.ToList();
        Assert.Equal(2, memberList.Count);
        Assert.Contains(memberList, m => m.source_entity_type == EntityTypes.Npc);
        Assert.Contains(memberList, m => m.source_entity_type == EntityTypes.Pc);
    }

    [Fact]
    public async Task OrganizationRivalry_TrackEnemyRelationships()
    {
        // Arrange
        var relationshipRepo = new FakeRelationshipRepository();

        var rivalry = new EntityRelationship
        {
            source_entity_type = EntityTypes.Organization,
            source_entity_id = 1,
            target_entity_type = EntityTypes.Organization,
            target_entity_id = 2,
            relationship_type_id = 3, // Enemy
            strength = 10,
            description = "Ancient blood feud",
            is_active = true
        };

        // Act
        await relationshipRepo.CreateRelationshipAsync(rivalry);
        var enemies = await relationshipRepo.GetRelationshipsByTypeAsync(EntityTypes.Organization, 1, 3);

        // Assert
        var enemyList = enemies.ToList();
        Assert.Single(enemyList);
        Assert.Equal(10, enemyList[0].strength);
    }

    [Fact]
    public async Task CampaignRelationships_FilterByCampaign()
    {
        // Arrange
        var relationshipRepo = new FakeRelationshipRepository();

        // Campaign 1 relationships
        await relationshipRepo.CreateRelationshipAsync(new EntityRelationship
        {
            source_entity_type = EntityTypes.Npc,
            source_entity_id = 1,
            target_entity_type = EntityTypes.Pc,
            target_entity_id = 2,
            relationship_type_id = 1,
            campaign_id = 1,
            is_active = true
        });

        await relationshipRepo.CreateRelationshipAsync(new EntityRelationship
        {
            source_entity_type = EntityTypes.Npc,
            source_entity_id = 3,
            target_entity_type = EntityTypes.Pc,
            target_entity_id = 4,
            relationship_type_id = 1,
            campaign_id = 1,
            is_active = true
        });

        // Campaign 2 relationship
        await relationshipRepo.CreateRelationshipAsync(new EntityRelationship
        {
            source_entity_type = EntityTypes.Npc,
            source_entity_id = 5,
            target_entity_type = EntityTypes.Pc,
            target_entity_id = 6,
            relationship_type_id = 1,
            campaign_id = 2,
            is_active = true
        });

        // Act
        var campaign1Rels = await relationshipRepo.GetRelationshipsByCampaignAsync(1);

        // Assert
        var relList = campaign1Rels.ToList();
        Assert.Equal(2, relList.Count);
        Assert.All(relList, r => Assert.Equal(1, r.campaign_id));
    }

    [Fact]
    public async Task SoftDelete_PreservesHistoryWhileHidingRelationship()
    {
        // Arrange
        var relationshipRepo = new FakeRelationshipRepository();

        var relationship = new EntityRelationship
        {
            source_entity_type = EntityTypes.Npc,
            source_entity_id = 1,
            target_entity_type = EntityTypes.Pc,
            target_entity_id = 2,
            relationship_type_id = 1,
            is_active = true
        };

        var id = await relationshipRepo.CreateRelationshipAsync(relationship);

        // Act - Deactivate instead of delete
        await relationshipRepo.DeactivateRelationshipAsync(id);

        // Assert - Still in database but not in active queries
        var allRels = await relationshipRepo.GetRelationshipsForEntityAsync(EntityTypes.Npc, 1, includeInactive: true);
        var activeRels = await relationshipRepo.GetRelationshipsForEntityAsync(EntityTypes.Npc, 1, includeInactive: false);

        Assert.Single(allRels);
        Assert.Empty(activeRels);
    }

    [Fact]
    public async Task MentorStudentRelationship_DirectionalityMatters()
    {
        // Arrange
        var relationshipRepo = new FakeRelationshipRepository();

        var mentorRel = new EntityRelationship
        {
            source_entity_type = EntityTypes.Npc,
            source_entity_id = 10, // Mentor NPC
            target_entity_type = EntityTypes.Pc,
            target_entity_id = 5,  // Student PC
            relationship_type_id = 5, // Mentor
            is_active = true
        };

        // Act
        await relationshipRepo.CreateRelationshipAsync(mentorRel);

        var fromMentor = await relationshipRepo.GetRelationshipsFromEntityAsync(EntityTypes.Npc, 10);
        var toStudent = await relationshipRepo.GetRelationshipsToEntityAsync(EntityTypes.Pc, 5);

        // Assert
        Assert.Single(fromMentor);
        Assert.Single(toStudent);
        Assert.Equal(fromMentor.First().entity_relationship_id, toStudent.First().entity_relationship_id);
    }

    [Fact]
    public async Task ComplexRelationshipWeb_QueriesWorkCorrectly()
    {
        // Arrange - Create a complex web of relationships
        var relationshipRepo = new FakeRelationshipRepository();

        // NPC1 is friends with PC1
        await relationshipRepo.CreateRelationshipAsync(new EntityRelationship
        {
            source_entity_type = EntityTypes.Npc,
            source_entity_id = 1,
            target_entity_type = EntityTypes.Pc,
            target_entity_id = 1,
            relationship_type_id = 1, // Friend
            is_active = true
        });

        // NPC1 is enemy of NPC2
        await relationshipRepo.CreateRelationshipAsync(new EntityRelationship
        {
            source_entity_type = EntityTypes.Npc,
            source_entity_id = 1,
            target_entity_type = EntityTypes.Npc,
            target_entity_id = 2,
            relationship_type_id = 3, // Enemy
            is_active = true
        });

        // NPC1 is member of Organization1
        await relationshipRepo.CreateRelationshipAsync(new EntityRelationship
        {
            source_entity_type = EntityTypes.Npc,
            source_entity_id = 1,
            target_entity_type = EntityTypes.Organization,
            target_entity_id = 1,
            relationship_type_id = 7, // Member
            is_active = true
        });

        // Act
        var npc1Relationships = await relationshipRepo.GetRelationshipsForEntityAsync(EntityTypes.Npc, 1);

        // Assert
        var relList = npc1Relationships.ToList();
        Assert.Equal(3, relList.Count);

        // Verify different target types
        Assert.Contains(relList, r => r.target_entity_type == EntityTypes.Pc);
        Assert.Contains(relList, r => r.target_entity_type == EntityTypes.Npc);
        Assert.Contains(relList, r => r.target_entity_type == EntityTypes.Organization);
    }
}
