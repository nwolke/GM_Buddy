using GM_Buddy.Contracts.Constants;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Contracts.Models.Npcs.Dnd;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GM_Buddy.Business;

public class NewAccountDataSeeder : INewAccountDataSeeder
{
    private readonly ILogger<NewAccountDataSeeder> _logger;
    private readonly INpcRepository _npcRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IGameSystemRepository _gamesystemRepository;
    private readonly IRelationshipRepository _relationshipRepository;

    public NewAccountDataSeeder(ILogger<NewAccountDataSeeder> logger,
        INpcRepository npcRepository,
        ICampaignRepository campaignRepository,
        IGameSystemRepository gameSystemRepository,
        IRelationshipRepository relationshipRepository)

    {
        _logger = logger;
        _campaignRepository = campaignRepository;
        _npcRepository = npcRepository;
        _gamesystemRepository = gameSystemRepository;
        _relationshipRepository = relationshipRepository;
    }

    public async Task SeedDefaultDataForNewAccountAsync(int accountId)
    {
        _logger.LogInformation("Seeding default data for new account {AccountId}", accountId);

        var defaultCampaign = new Campaign
        {
            account_id = accountId,
            name = "The Heroes Adventure",
            description = "A beginner-friendly adventure in the world of GM Buddy"
        };
        int campaignId = await _campaignRepository.CreateAsync(defaultCampaign);

        // Create a default NPC 1
        var defaultNpc1 = new Npc
        {
            campaign_id = campaignId,
            account_id = accountId,
            name = "Gorath the Brave",
            description = "A fearless warrior from the northern tribes",
            stats = JsonSerializer.Serialize(new DnDStats
            {
                Lineage = "Human",
                Occupation = "Fighter"
            })
        };
        var npcId1 = await _npcRepository.CreateNpcAsync(defaultNpc1);

        // Create a default NPC 2
        var defaultNpc2 = new Npc
        {
            campaign_id = campaignId,
            account_id = accountId,
            name = "Lathel Spellbinder",
            description = "An intelligent elf wizard from the forests of Eldoria",
            stats = JsonSerializer.Serialize(new DnDStats
            {
                Lineage = "Elf",
                Occupation = "Wizard"
            })
        };
        var npcId2 = await _npcRepository.CreateNpcAsync(defaultNpc2);

        var allyRelationshipType = await _relationshipRepository.GetRelationshipTypeByNameAsync("Ally");
        if (allyRelationshipType == null)
        {
            _logger.LogError("Relationship Type 'Ally' not found. Aborting relationship creation for account {AccountId}.", accountId);
            throw new InvalidOperationException("Ally Relationship Type not found.");
        }

        var relationship = new EntityRelationship
        {
            campaign_id = campaignId,
            source_entity_type = EntityTypes.Npc,
            source_entity_id = npcId1,
            target_entity_type = EntityTypes.Npc,
            target_entity_id = npcId2,
            relationship_type_id = allyRelationshipType.relationship_type_id,
            description = "Gorath and Lathel have formed a strong alliance to face the challenges ahead.",
            is_directional = false,
            is_active = true,
            strength = 8
        };
        await _relationshipRepository.CreateRelationshipAsync(relationship);
    }
}
