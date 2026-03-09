using GM_Buddy.Contracts.DbEntities;

namespace GM_Buddy.Business.UnitTests;

/// <summary>
/// Unit tests for Relationship Repository functionality
/// </summary>
public class RelationshipRepositoryTests
{
    #region Relationship Type Tests

    [Fact]
    public async Task GetAllRelationshipTypes_ReturnsDefaultTypes()
    {
        // Arrange
        var repo = new FakeRelationshipRepository();

        // Act
        var result = await repo.GetAllRelationshipTypesAsync();

        // Assert
        var types = result.ToList();
        Assert.NotEmpty(types);
        Assert.Contains(types, t => t.relationship_type_name == "Friend");
        Assert.Contains(types, t => t.relationship_type_name == "Enemy");
        Assert.Contains(types, t => t.relationship_type_name == "Patron");
        Assert.Equal(24, types.Count);
    }

    [Fact]
    public async Task GetRelationshipTypeById_ReturnsCorrectType()
    {
        // Arrange
        var repo = new FakeRelationshipRepository();

        // Act
        var result = await repo.GetRelationshipTypeByIdAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Friend", result.relationship_type_name);
    }

    [Fact]
    public async Task GetRelationshipTypeByName_IsCaseInsensitive()
    {
        // Arrange
        var repo = new FakeRelationshipRepository();

        // Act
        var result = await repo.GetRelationshipTypeByNameAsync("FRIEND");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.relationship_type_id);
    }

    [Fact]
    public async Task GetRelationshipTypeByName_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var repo = new FakeRelationshipRepository();

        // Act
        var result = await repo.GetRelationshipTypeByNameAsync("NonExistent");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Create Relationship Tests

    [Fact]
    public async Task CreateRelationship_AssignsNewId()
    {
        // Arrange
        var repo = new FakeRelationshipRepository();
        var relationship = new EntityRelationship
        {
            source_entity_type = "npc",
            source_entity_id = 1,
            target_entity_type = "pc",
            target_entity_id = 2,
            relationship_type_id = 1
        };

        // Act
        var id = await repo.CreateRelationshipAsync(relationship);

        // Assert
        Assert.True(id > 0);
        Assert.Equal(id, relationship.entity_relationship_id);
    }

    [Fact]
    public async Task CreateRelationship_SetsTimestamps()
    {
        // Arrange
        var repo = new FakeRelationshipRepository();
        var relationship = new EntityRelationship
        {
            source_entity_type = "npc",
            source_entity_id = 1,
            target_entity_type = "pc",
            target_entity_id = 2,
            relationship_type_id = 1
        };
        var beforeCreate = DateTime.UtcNow;

        // Act
        await repo.CreateRelationshipAsync(relationship);

        // Assert
        Assert.True(relationship.created_at >= beforeCreate);
        Assert.True(relationship.updated_at >= beforeCreate);
    }

    [Fact]
    public async Task CreateRelationship_CanBeRetrievedById()
    {
        // Arrange
        var repo = new FakeRelationshipRepository();
        var relationship = new EntityRelationship
        {
            source_entity_type = "npc",
            source_entity_id = 1,
            target_entity_type = "pc",
            target_entity_id = 2,
            relationship_type_id = 1,
            strength = 8
        };

        // Act
        var id = await repo.CreateRelationshipAsync(relationship);
        var retrieved = await repo.GetRelationshipByIdAsync(id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(8, retrieved.strength);
    }

    #endregion

    #region Query Relationship Tests

    [Fact]
    public async Task GetRelationshipsForEntity_ReturnsSourceAndTargetMatches()
    {
        // Arrange
        var relationships = new[]
        {
            new EntityRelationship
            {
                entity_relationship_id = 1,
                source_entity_type = "npc",
                source_entity_id = 1,
                target_entity_type = "pc",
                target_entity_id = 2,
                relationship_type_id = 1,
                is_active = true
            },
            new EntityRelationship
            {
                entity_relationship_id = 2,
                source_entity_type = "pc",
                source_entity_id = 2,
                target_entity_type = "organization",
                target_entity_id = 3,
                relationship_type_id = 7,
                is_active = true
            }
        };
        var repo = new FakeRelationshipRepository(relationships: relationships);

        // Act
        var result = await repo.GetRelationshipsForEntityAsync("pc", 2);

        // Assert
        var relList = result.ToList();
        Assert.Equal(2, relList.Count);
    }

    [Fact]
    public async Task GetRelationshipsForEntity_ExcludesInactive_ByDefault()
    {
        // Arrange
        var relationships = new[]
        {
            new EntityRelationship
            {
                entity_relationship_id = 1,
                source_entity_type = "npc",
                source_entity_id = 1,
                target_entity_type = "pc",
                target_entity_id = 2,
                relationship_type_id = 1,
                is_active = true
            },
            new EntityRelationship
            {
                entity_relationship_id = 2,
                source_entity_type = "npc",
                source_entity_id = 1,
                target_entity_type = "pc",
                target_entity_id = 3,
                relationship_type_id = 1,
                is_active = false
            }
        };
        var repo = new FakeRelationshipRepository(relationships: relationships);

        // Act
        var result = await repo.GetRelationshipsForEntityAsync("npc", 1);

        // Assert
        var relList = result.ToList();
        Assert.Single(relList);
        Assert.True(relList[0].is_active);
    }

    [Fact]
    public async Task GetRelationshipsForEntity_IncludesInactive_WhenRequested()
    {
        // Arrange
        var relationships = new[]
        {
            new EntityRelationship
            {
                entity_relationship_id = 1,
                source_entity_type = "npc",
                source_entity_id = 1,
                target_entity_type = "pc",
                target_entity_id = 2,
                relationship_type_id = 1,
                is_active = true
            },
            new EntityRelationship
            {
                entity_relationship_id = 2,
                source_entity_type = "npc",
                source_entity_id = 1,
                target_entity_type = "pc",
                target_entity_id = 3,
                relationship_type_id = 1,
                is_active = false
            }
        };
        var repo = new FakeRelationshipRepository(relationships: relationships);

        // Act
        var result = await repo.GetRelationshipsForEntityAsync("npc", 1, includeInactive: true);

        // Assert
        var relList = result.ToList();
        Assert.Equal(2, relList.Count);
    }

    [Fact]
    public async Task GetRelationshipsFromEntity_ReturnsOnlySourceMatches()
    {
        // Arrange
        var relationships = new[]
        {
            new EntityRelationship
            {
                entity_relationship_id = 1,
                source_entity_type = "npc",
                source_entity_id = 1,
                target_entity_type = "pc",
                target_entity_id = 2,
                relationship_type_id = 1,
                is_active = true
            },
            new EntityRelationship
            {
                entity_relationship_id = 2,
                source_entity_type = "pc",
                source_entity_id = 2,
                target_entity_type = "npc",
                target_entity_id = 1,
                relationship_type_id = 1,
                is_active = true
            }
        };
        var repo = new FakeRelationshipRepository(relationships: relationships);

        // Act
        var result = await repo.GetRelationshipsFromEntityAsync("npc", 1);

        // Assert
        var relList = result.ToList();
        Assert.Single(relList);
        Assert.Equal("pc", relList[0].target_entity_type);
    }

    [Fact]
    public async Task GetRelationshipsToEntity_ReturnsOnlyTargetMatches()
    {
        // Arrange
        var relationships = new[]
        {
            new EntityRelationship
            {
                entity_relationship_id = 1,
                source_entity_type = "npc",
                source_entity_id = 1,
                target_entity_type = "organization",
                target_entity_id = 5,
                relationship_type_id = 7,
                is_active = true
            },
            new EntityRelationship
            {
                entity_relationship_id = 2,
                source_entity_type = "pc",
                source_entity_id = 2,
                target_entity_type = "organization",
                target_entity_id = 5,
                relationship_type_id = 7,
                is_active = true
            }
        };
        var repo = new FakeRelationshipRepository(relationships: relationships);

        // Act
        var result = await repo.GetRelationshipsToEntityAsync("organization", 5);

        // Assert
        var relList = result.ToList();
        Assert.Equal(2, relList.Count);
        Assert.All(relList, r => Assert.Equal("organization", r.target_entity_type));
    }

    [Fact]
    public async Task GetRelationshipsByType_FiltersCorrectly()
    {
        // Arrange
        var relationships = new[]
        {
            new EntityRelationship
            {
                entity_relationship_id = 1,
                source_entity_type = "npc",
                source_entity_id = 1,
                target_entity_type = "pc",
                target_entity_id = 2,
                relationship_type_id = 10, // Friend
                is_active = true
            },
            new EntityRelationship
            {
                entity_relationship_id = 2,
                source_entity_type = "npc",
                source_entity_id = 1,
                target_entity_type = "pc",
                target_entity_id = 3,
                relationship_type_id = 7, // Enemy
                is_active = true
            }
        };
        var repo = new FakeRelationshipRepository(relationships: relationships);

        // Act
        var result = await repo.GetRelationshipsByTypeAsync("npc", 1, 10); // Friend type

        // Assert
        var relList = result.ToList();
        Assert.Single(relList);
        Assert.Equal(10, relList[0].relationship_type_id);
    }

    [Fact]
    public async Task GetRelationshipsByCampaign_FiltersCorrectly()
    {
        // Arrange
        var relationships = new[]
        {
            new EntityRelationship
            {
                entity_relationship_id = 1,
                source_entity_type = "npc",
                source_entity_id = 1,
                target_entity_type = "pc",
                target_entity_id = 2,
                relationship_type_id = 1,
                campaign_id = 1,
                is_active = true
            },
            new EntityRelationship
            {
                entity_relationship_id = 2,
                source_entity_type = "npc",
                source_entity_id = 3,
                target_entity_type = "pc",
                target_entity_id = 4,
                relationship_type_id = 1,
                campaign_id = 2,
                is_active = true
            }
        };
        var repo = new FakeRelationshipRepository(relationships: relationships);

        // Act
        var result = await repo.GetRelationshipsByCampaignAsync(1);

        // Assert
        var relList = result.ToList();
        Assert.Single(relList);
        Assert.Equal(1, relList[0].campaign_id);
    }

    #endregion

    #region Attitude Score Tests

    [Fact]
    public async Task CreateRelationship_PersistsAttitudeScore()
    {
        // Arrange
        var repo = new FakeRelationshipRepository();
        var relationship = new EntityRelationship
        {
            source_entity_type = "npc",
            source_entity_id = 1,
            target_entity_type = "pc",
            target_entity_id = 2,
            relationship_type_id = 10,
            attitude_score = 3
        };

        // Act
        var id = await repo.CreateRelationshipAsync(relationship);
        var retrieved = await repo.GetRelationshipByIdAsync(id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(3, retrieved.attitude_score);
    }

    [Fact]
    public async Task UpdateRelationship_PersistsAttitudeScoreAndCustomType()
    {
        // Arrange
        var relationship = new EntityRelationship
        {
            entity_relationship_id = 1,
            source_entity_type = "npc",
            source_entity_id = 1,
            target_entity_type = "pc",
            target_entity_id = 2,
            relationship_type_id = 10,
            attitude_score = 0,
            is_active = true
        };
        var repo = new FakeRelationshipRepository(relationships: new[] { relationship });

        var updated = new EntityRelationship
        {
            entity_relationship_id = 1,
            source_entity_type = "npc",
            source_entity_id = 1,
            target_entity_type = "pc",
            target_entity_id = 2,
            relationship_type_id = 10,
            attitude_score = -4,
            custom_type = "Blood Oath",
            is_active = true
        };

        // Act
        await repo.UpdateRelationshipAsync(updated);
        var result = await repo.GetRelationshipByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(-4, result.attitude_score);
        Assert.Equal("Blood Oath", result.custom_type);
    }

    #endregion

    #region PC Stances Tests

    [Fact]
    public async Task GetPcStancesForNpc_ReturnsOnlyPcRelationships()
    {
        // Arrange
        var relationships = new[]
        {
            new EntityRelationship
            {
                entity_relationship_id = 1,
                source_entity_type = "npc",
                source_entity_id = 1,
                target_entity_type = "pc",
                target_entity_id = 10,
                relationship_type_id = 10,
                attitude_score = 3,
                is_active = true
            },
            new EntityRelationship
            {
                entity_relationship_id = 2,
                source_entity_type = "pc",
                source_entity_id = 20,
                target_entity_type = "npc",
                target_entity_id = 1,
                relationship_type_id = 7,
                attitude_score = -2,
                is_active = true
            },
            new EntityRelationship
            {
                entity_relationship_id = 3,
                source_entity_type = "npc",
                source_entity_id = 1,
                target_entity_type = "npc",
                target_entity_id = 5,
                relationship_type_id = 2,
                attitude_score = 1,
                is_active = true
            }
        };
        var repo = new FakeRelationshipRepository(relationships: relationships);

        // Act
        var result = (await repo.GetPcStancesForNpcAsync(1)).ToList();

        // Assert — only relationships 1 and 2 involve NPC 1 ↔ PC
        Assert.Equal(2, result.Count);
        Assert.All(result, r =>
            Assert.True(
                (r.source_entity_type == "pc" || r.target_entity_type == "pc") &&
                (r.source_entity_type == "npc" || r.target_entity_type == "npc")));
    }

    [Fact]
    public async Task GetPcStancesForNpc_RespectsCampaignFilter()
    {
        // Arrange
        var relationships = new[]
        {
            new EntityRelationship
            {
                entity_relationship_id = 1,
                source_entity_type = "npc",
                source_entity_id = 1,
                target_entity_type = "pc",
                target_entity_id = 10,
                relationship_type_id = 10,
                campaign_id = 1,
                is_active = true
            },
            new EntityRelationship
            {
                entity_relationship_id = 2,
                source_entity_type = "npc",
                source_entity_id = 1,
                target_entity_type = "pc",
                target_entity_id = 20,
                relationship_type_id = 7,
                campaign_id = 2,
                is_active = true
            }
        };
        var repo = new FakeRelationshipRepository(relationships: relationships);

        // Act
        var result = (await repo.GetPcStancesForNpcAsync(1, campaignId: 1)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0].campaign_id);
    }

    [Fact]
    public async Task GetPcStancesForNpc_ExcludesInactive()
    {
        // Arrange
        var relationships = new[]
        {
            new EntityRelationship
            {
                entity_relationship_id = 1,
                source_entity_type = "npc",
                source_entity_id = 1,
                target_entity_type = "pc",
                target_entity_id = 10,
                relationship_type_id = 10,
                is_active = true
            },
            new EntityRelationship
            {
                entity_relationship_id = 2,
                source_entity_type = "npc",
                source_entity_id = 1,
                target_entity_type = "pc",
                target_entity_id = 20,
                relationship_type_id = 7,
                is_active = false
            }
        };
        var repo = new FakeRelationshipRepository(relationships: relationships);

        // Act
        var result = (await repo.GetPcStancesForNpcAsync(1)).ToList();

        // Assert
        Assert.Single(result);
        Assert.True(result[0].is_active);
    }

    #endregion

    #region Update/Delete Relationship Tests

    [Fact]
    public async Task UpdateRelationship_ModifiesExistingRelationship()
    {
        // Arrange
        var relationship = new EntityRelationship
        {
            entity_relationship_id = 1,
            source_entity_type = "npc",
            source_entity_id = 1,
            target_entity_type = "pc",
            target_entity_id = 2,
            relationship_type_id = 1,
            strength = 5,
            is_active = true
        };
        var repo = new FakeRelationshipRepository(relationships: new[] { relationship });

        var updated = new EntityRelationship
        {
            entity_relationship_id = 1,
            source_entity_type = "npc",
            source_entity_id = 1,
            target_entity_type = "pc",
            target_entity_id = 2,
            strength = 10,
            description = "Best friends now",
            is_active = true,
            relationship_type_id = 1
        };

        // Act
        await repo.UpdateRelationshipAsync(updated);
        var result = await repo.GetRelationshipByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.strength);
        Assert.Equal("Best friends now", result.description);
    }

    [Fact]
    public async Task DeleteRelationship_RemovesRelationship()
    {
        // Arrange
        var relationship = new EntityRelationship
        {
            entity_relationship_id = 1,
            source_entity_type = "npc",
            source_entity_id = 1,
            target_entity_type = "pc",
            target_entity_id = 2,
            relationship_type_id = 1,
            is_active = true
        };
        var repo = new FakeRelationshipRepository(relationships: new[] { relationship });

        // Act
        await repo.DeleteRelationshipAsync(1);
        var result = await repo.GetRelationshipByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeactivateRelationship_SetsInactive()
    {
        // Arrange
        var relationship = new EntityRelationship
        {
            entity_relationship_id = 1,
            source_entity_type = "npc",
            source_entity_id = 1,
            target_entity_type = "pc",
            target_entity_id = 2,
            relationship_type_id = 1,
            is_active = true
        };
        var repo = new FakeRelationshipRepository(relationships: new[] { relationship });

        // Act
        await repo.DeactivateRelationshipAsync(1);
        var result = await repo.GetRelationshipByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.is_active);
    }

    [Fact]
    public async Task ReactivateRelationship_SetsActive()
    {
        // Arrange
        var relationship = new EntityRelationship
        {
            entity_relationship_id = 1,
            source_entity_type = "npc",
            source_entity_id = 1,
            target_entity_type = "pc",
            target_entity_id = 2,
            relationship_type_id = 1,
            is_active = false
        };
        var repo = new FakeRelationshipRepository(relationships: new[] { relationship });

        // Act
        await repo.ReactivateRelationshipAsync(1);
        var result = await repo.GetRelationshipByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.is_active);
    }

    [Fact]
    public async Task RelationshipExists_ReturnsTrue_WhenRelationshipExists()
    {
        // Arrange
        var relationship = new EntityRelationship
        {
            entity_relationship_id = 1,
            source_entity_type = "npc",
            source_entity_id = 1,
            target_entity_type = "pc",
            target_entity_id = 2,
            relationship_type_id = 1,
            is_active = true
        };
        var repo = new FakeRelationshipRepository(relationships: new[] { relationship });

        // Act
        var exists = await repo.RelationshipExistsAsync("npc", 1, "pc", 2, 1);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task RelationshipExists_ReturnsFalse_WhenRelationshipDoesNotExist()
    {
        // Arrange
        var repo = new FakeRelationshipRepository();

        // Act
        var exists = await repo.RelationshipExistsAsync("npc", 1, "pc", 2, 1);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task RelationshipExists_ReturnsFalse_WhenRelationshipInactive()
    {
        // Arrange
        var relationship = new EntityRelationship
        {
            entity_relationship_id = 1,
            source_entity_type = "npc",
            source_entity_id = 1,
            target_entity_type = "pc",
            target_entity_id = 2,
            relationship_type_id = 1,
            is_active = false
        };
        var repo = new FakeRelationshipRepository(relationships: new[] { relationship });

        // Act
        var exists = await repo.RelationshipExistsAsync("npc", 1, "pc", 2, 1);

        // Assert
        Assert.False(exists);
    }

    #endregion
}
