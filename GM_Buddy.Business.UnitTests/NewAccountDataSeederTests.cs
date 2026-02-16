using GM_Buddy.Contracts.Constants;
using GM_Buddy.Contracts.DbEntities;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace GM_Buddy.Business.UnitTests;

/// <summary>
/// Unit tests for NewAccountDataSeeder class
/// </summary>
public class NewAccountDataSeederTests
{
    [Fact]
    public async Task SeedDefaultDataForNewAccountAsync_CreatesDefaultCampaignWithGenericGameSystem()
    {
        // Arrange
        var npcRepo = new FakeNpcRepository();
        var campaignRepo = new FakeCampaignRepository();
        var gameSystemRepo = new FakeGameSystemRepository();
        var relationshipRepo = new FakeRelationshipRepository();
        
        var seeder = new NewAccountDataSeeder(
            NullLogger<NewAccountDataSeeder>.Instance,
            npcRepo,
            campaignRepo,
            gameSystemRepo,
            relationshipRepo
        );

        int testAccountId = 999;

        // Act
        await seeder.SeedDefaultDataForNewAccountAsync(testAccountId);

        // Assert
        var campaigns = await campaignRepo.GetByAccountIdAsync(testAccountId);
        var campaignList = campaigns.ToList();
        
        Assert.Single(campaignList);
        Assert.Equal("The Heroes Adventure", campaignList[0].name);
        Assert.Equal("A beginner-friendly adventure in the world of GM Buddy", campaignList[0].description);
        Assert.Equal(testAccountId, campaignList[0].account_id);
        
        // Verify it uses Generic game system
        var genericSystem = await gameSystemRepo.GetByNameAsync("Generic");
        Assert.NotNull(genericSystem);
    }

    [Fact]
    public async Task SeedDefaultDataForNewAccountAsync_CreatesTwoDefaultNpcs()
    {
        // Arrange
        var npcRepo = new FakeNpcRepository();
        var campaignRepo = new FakeCampaignRepository();
        var gameSystemRepo = new FakeGameSystemRepository();
        var relationshipRepo = new FakeRelationshipRepository();
        
        var seeder = new NewAccountDataSeeder(
            NullLogger<NewAccountDataSeeder>.Instance,
            npcRepo,
            campaignRepo,
            gameSystemRepo,
            relationshipRepo
        );

        int testAccountId = 999;

        // Act
        await seeder.SeedDefaultDataForNewAccountAsync(testAccountId);

        // Assert
        var npcs = await npcRepo.GetNpcs(testAccountId, null);
        var npcList = npcs.ToList();
        
        Assert.Equal(2, npcList.Count);
        
        // Verify first NPC (Gorath the Brave)
        var gorath = npcList.FirstOrDefault(n => n.name == "Gorath the Brave");
        Assert.NotNull(gorath);
        Assert.Equal("A fearless warrior from the northern tribes", gorath.description);
        Assert.Equal(testAccountId, gorath.account_id);
        
        // Verify Gorath's stats are valid JSON
        var gorathStats = JsonSerializer.Deserialize<Dictionary<string, string>>(gorath.stats);
        Assert.NotNull(gorathStats);
        Assert.Equal("Human", gorathStats["lineage"]);
        Assert.Equal("Fighter", gorathStats["occupation"]);
        
        // Verify second NPC (Lathel Spellbinder)
        var lathel = npcList.FirstOrDefault(n => n.name == "Lathel Spellbinder");
        Assert.NotNull(lathel);
        Assert.Equal("An intelligent elf wizard from the forests of Eldoria", lathel.description);
        Assert.Equal(testAccountId, lathel.account_id);
        
        // Verify Lathel's stats are valid JSON
        var lathelStats = JsonSerializer.Deserialize<Dictionary<string, string>>(lathel.stats);
        Assert.NotNull(lathelStats);
        Assert.Equal("Elf", lathelStats["lineage"]);
        Assert.Equal("Wizard", lathelStats["occupation"]);
    }

    [Fact]
    public async Task SeedDefaultDataForNewAccountAsync_NpcsAreAssignedToCreatedCampaign()
    {
        // Arrange
        var npcRepo = new FakeNpcRepository();
        var campaignRepo = new FakeCampaignRepository();
        var gameSystemRepo = new FakeGameSystemRepository();
        var relationshipRepo = new FakeRelationshipRepository();
        
        var seeder = new NewAccountDataSeeder(
            NullLogger<NewAccountDataSeeder>.Instance,
            npcRepo,
            campaignRepo,
            gameSystemRepo,
            relationshipRepo
        );

        int testAccountId = 999;

        // Act
        await seeder.SeedDefaultDataForNewAccountAsync(testAccountId);

        // Assert
        var campaigns = await campaignRepo.GetByAccountIdAsync(testAccountId);
        var campaign = campaigns.First();
        
        var npcs = await npcRepo.GetNpcs(testAccountId, campaign.campaign_id);
        var npcList = npcs.ToList();
        
        Assert.Equal(2, npcList.Count);
        Assert.All(npcList, npc => Assert.Equal(campaign.campaign_id, npc.campaign_id));
    }

    [Fact]
    public async Task SeedDefaultDataForNewAccountAsync_CreatesAllyRelationshipBetweenNpcs()
    {
        // Arrange
        var npcRepo = new FakeNpcRepository();
        var campaignRepo = new FakeCampaignRepository();
        var gameSystemRepo = new FakeGameSystemRepository();
        var relationshipRepo = new FakeRelationshipRepository();
        
        var seeder = new NewAccountDataSeeder(
            NullLogger<NewAccountDataSeeder>.Instance,
            npcRepo,
            campaignRepo,
            gameSystemRepo,
            relationshipRepo
        );

        int testAccountId = 999;

        // Act
        await seeder.SeedDefaultDataForNewAccountAsync(testAccountId);

        // Assert
        var campaigns = await campaignRepo.GetByAccountIdAsync(testAccountId);
        var campaign = campaigns.First();
        
        var relationships = await relationshipRepo.GetRelationshipsByCampaignAsync(campaign.campaign_id);
        var relationshipList = relationships.ToList();
        
        Assert.Single(relationshipList);
        
        var relationship = relationshipList[0];
        Assert.Equal(EntityTypes.Npc, relationship.source_entity_type);
        Assert.Equal(EntityTypes.Npc, relationship.target_entity_type);
        Assert.Equal("Gorath and Lathel have formed a strong alliance to face the challenges ahead.", relationship.description);
        Assert.False(relationship.is_directional);
        Assert.True(relationship.is_active);
        Assert.Equal(8, relationship.strength);
        
        // Verify it's using the "Ally" relationship type
        var allyType = await relationshipRepo.GetRelationshipTypeByNameAsync("Ally");
        Assert.NotNull(allyType);
        Assert.Equal(allyType.relationship_type_id, relationship.relationship_type_id);
    }

    [Fact]
    public async Task SeedDefaultDataForNewAccountAsync_ThrowsExceptionWhenGenericGameSystemNotFound()
    {
        // Arrange
        var npcRepo = new FakeNpcRepository();
        var campaignRepo = new FakeCampaignRepository();
        var emptyGameSystemRepo = new FakeGameSystemRepository(Array.Empty<Game_System>()); // No game systems
        var relationshipRepo = new FakeRelationshipRepository();
        
        var seeder = new NewAccountDataSeeder(
            NullLogger<NewAccountDataSeeder>.Instance,
            npcRepo,
            campaignRepo,
            emptyGameSystemRepo,
            relationshipRepo
        );

        int testAccountId = 999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await seeder.SeedDefaultDataForNewAccountAsync(testAccountId)
        );
        
        Assert.Equal("Generic Game System not found.", exception.Message);
    }

    [Fact]
    public async Task SeedDefaultDataForNewAccountAsync_ThrowsExceptionWhenAllyRelationshipTypeNotFound()
    {
        // Arrange
        var npcRepo = new FakeNpcRepository();
        var campaignRepo = new FakeCampaignRepository();
        var gameSystemRepo = new FakeGameSystemRepository();
        var emptyRelationshipRepo = new FakeRelationshipRepository(
            relationshipTypes: Array.Empty<RelationshipType>() // No relationship types
        );
        
        var seeder = new NewAccountDataSeeder(
            NullLogger<NewAccountDataSeeder>.Instance,
            npcRepo,
            campaignRepo,
            gameSystemRepo,
            emptyRelationshipRepo
        );

        int testAccountId = 999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await seeder.SeedDefaultDataForNewAccountAsync(testAccountId)
        );
        
        Assert.Equal("Ally Relationship Type not found.", exception.Message);
    }

    [Fact]
    public async Task SeedDefaultDataForNewAccountAsync_MultipleAccountsGetIndependentData()
    {
        // Arrange
        var npcRepo = new FakeNpcRepository();
        var campaignRepo = new FakeCampaignRepository();
        var gameSystemRepo = new FakeGameSystemRepository();
        var relationshipRepo = new FakeRelationshipRepository();
        
        var seeder = new NewAccountDataSeeder(
            NullLogger<NewAccountDataSeeder>.Instance,
            npcRepo,
            campaignRepo,
            gameSystemRepo,
            relationshipRepo
        );

        // Act - Seed data for two different accounts
        await seeder.SeedDefaultDataForNewAccountAsync(100);
        await seeder.SeedDefaultDataForNewAccountAsync(200);

        // Assert - Each account has its own campaign and NPCs
        var account100Campaigns = await campaignRepo.GetByAccountIdAsync(100);
        var account200Campaigns = await campaignRepo.GetByAccountIdAsync(200);
        
        Assert.Single(account100Campaigns);
        Assert.Single(account200Campaigns);
        Assert.NotEqual(account100Campaigns.First().campaign_id, account200Campaigns.First().campaign_id);
        
        var account100Npcs = await npcRepo.GetNpcs(100, null);
        var account200Npcs = await npcRepo.GetNpcs(200, null);
        
        Assert.Equal(2, account100Npcs.Count());
        Assert.Equal(2, account200Npcs.Count());
        Assert.All(account100Npcs, npc => Assert.Equal(100, npc.account_id));
        Assert.All(account200Npcs, npc => Assert.Equal(200, npc.account_id));
    }
}

