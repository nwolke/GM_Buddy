using GM_Buddy.Business.ComponentTests.Fakes;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Models.Npcs;
using GM_Buddy.Contracts.Models.Npcs.Dnd;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace GM_Buddy.Business.ComponentTests.StepDefinitions;

[Binding]
public class NpcManagementStepDefinitions
{
    private readonly FakeNpcRepository _npcRepository;
    private readonly FakeCampaignRepository _campaignRepository;
    private readonly NpcLogic _npcLogic;

    private int _currentAccountId;
    private int _currentCampaignId;
    private int _createdNpcId;
    private bool _operationSuccess;
    private Exception? _caughtException;
    private IEnumerable<DndNpc>? _retrievedNpcs;
    private DndNpc? _retrievedNpc;

    public NpcManagementStepDefinitions()
    {
        _npcRepository = new FakeNpcRepository();
        _campaignRepository = new FakeCampaignRepository();
        _npcLogic = new NpcLogic(
            _npcRepository,
            _campaignRepository,
            NullLogger<NpcLogic>.Instance
        );
    }

    [Given(@"I have a valid campaign with ID (.*) for account (.*)")]
    public void GivenIHaveAValidCampaignWithIdForAccount(int campaignId, int accountId)
    {
        _currentAccountId = accountId;
        _currentCampaignId = campaignId;

        var campaign = new Campaign
        {
            campaign_id = campaignId,
            account_id = accountId,
            name = "Test Campaign",
            description = "A test campaign"
        };
        _campaignRepository.AddCampaign(campaign);
    }

    [Given(@"no campaign exists with ID (.*)")]
    public void GivenNoCampaignExistsWithId(int campaignId)
    {
        _currentCampaignId = campaignId;
        // Campaign repository is empty by default
    }

    [Given(@"I have a campaign with ID (.*) belonging to account (.*)")]
    public void GivenIHaveACampaignWithIdBelongingToAccount(int campaignId, int accountId)
    {
        _currentCampaignId = campaignId;

        var campaign = new Campaign
        {
            campaign_id = campaignId,
            account_id = accountId,
            name = "Other Account's Campaign",
            description = "Belongs to a different account"
        };
        _campaignRepository.AddCampaign(campaign);
    }

    [Given(@"I have an existing NPC with ID (.*) for account (.*)")]
    public void GivenIHaveAnExistingNpcWithIdForAccount(int npcId, int accountId)
    {
        _currentAccountId = accountId;
        var campaignId = 1;
        _currentCampaignId = campaignId;

        // Ensure campaign exists
        var campaign = new Campaign
        {
            campaign_id = campaignId,
            account_id = accountId,
            name = "Test Campaign",
            description = "A test campaign"
        };
        _campaignRepository.AddCampaign(campaign);

        var stats = new
        {
            lineage = "Dwarf",
            occupation = "Warrior",
            faction = "Lonely Mountain",
            notes = ""
        };

        var npc = new Npc
        {
            npc_id = npcId,
            account_id = accountId,
            campaign_id = campaignId,
            name = "Test NPC",
            description = "An existing NPC",
            stats = JsonSerializer.Serialize(stats)
        };
        _npcRepository.AddNpc(npc);
    }

    [Given(@"I have an existing NPC with ID (.*)")]
    public void GivenIHaveAnExistingNpcWithId(int npcId)
    {
        GivenIHaveAnExistingNpcWithIdForAccount(npcId, 100);
    }

    [Given(@"I have an existing NPC with ID (.*) named ""(.*)""")]
    public void GivenIHaveAnExistingNpcWithIdNamed(int npcId, string name)
    {
        var accountId = 100;
        var campaignId = 1;

        var campaign = new Campaign
        {
            campaign_id = campaignId,
            account_id = accountId,
            name = "Test Campaign",
            description = "A test campaign"
        };
        _campaignRepository.AddCampaign(campaign);

        var stats = new
        {
            lineage = "Dwarf",
            occupation = "Warrior",
            faction = "",
            notes = ""
        };

        var npc = new Npc
        {
            npc_id = npcId,
            account_id = accountId,
            campaign_id = campaignId,
            name = name,
            description = "Test NPC",
            stats = JsonSerializer.Serialize(stats)
        };
        _npcRepository.AddNpc(npc);
    }

    [Given(@"I have a campaign with ID (.*) for account (.*)")]
    public void GivenIHaveACampaignWithIdForAccount(int campaignId, int accountId)
    {
        _currentAccountId = accountId;
        _currentCampaignId = campaignId;

        var campaign = new Campaign
        {
            campaign_id = campaignId,
            account_id = accountId,
            name = "Test Campaign",
            description = "A test campaign"
        };
        _campaignRepository.AddCampaign(campaign);
    }

    [Given(@"the campaign has the following NPCs:")]
    public void GivenTheCampaignHasTheFollowingNpcs(Table table)
    {
        foreach (var row in table.Rows)
        {
            var stats = new
            {
                lineage = row["Race"],
                occupation = row["Class"],
                faction = "",
                notes = ""
            };

            var npc = new Npc
            {
                account_id = _currentAccountId,
                campaign_id = _currentCampaignId,
                name = row["Name"],
                description = "",
                stats = JsonSerializer.Serialize(stats)
            };
            _npcRepository.AddNpc(npc);
        }
    }

    [When(@"I create an NPC with the following details:")]
    public async Task WhenICreateAnNpcWithTheFollowingDetails(Table table)
    {
        try
        {
            var data = table.CreateInstance<CreateNpcData>();
            var request = new CreateNpcRequest
            {
                CampaignId = _currentCampaignId,
                Name = data.Name,
                Description = data.Description,
                Race = data.Race,
                Class = data.Class,
                Faction = data.Faction
            };

            _createdNpcId = await _npcLogic.CreateNpcAsync(_currentAccountId, request);
            _operationSuccess = true;
        }
        catch (Exception ex)
        {
            _caughtException = ex;
            _operationSuccess = false;
        }
    }

    [When(@"I attempt to create an NPC for campaign (.*)")]
    public async Task WhenIAttemptToCreateAnNpcForCampaign(int campaignId)
    {
        try
        {
            var request = new CreateNpcRequest
            {
                CampaignId = campaignId,
                Name = "Test NPC",
                Description = "Test",
                Race = "Human",
                Class = "Fighter"
            };

            _createdNpcId = await _npcLogic.CreateNpcAsync(100, request);
            _operationSuccess = true;
        }
        catch (Exception ex)
        {
            _caughtException = ex;
            _operationSuccess = false;
        }
    }

    [When(@"I attempt to create an NPC for campaign (.*) with account (.*)")]
    public async Task WhenIAttemptToCreateAnNpcForCampaignWithAccount(int campaignId, int accountId)
    {
        try
        {
            var request = new CreateNpcRequest
            {
                CampaignId = campaignId,
                Name = "Test NPC",
                Description = "Test",
                Race = "Human",
                Class = "Fighter"
            };

            _createdNpcId = await _npcLogic.CreateNpcAsync(accountId, request);
            _operationSuccess = true;
        }
        catch (Exception ex)
        {
            _caughtException = ex;
            _operationSuccess = false;
        }
    }

    [When(@"I update the NPC with the following details:")]
    public async Task WhenIUpdateTheNpcWithTheFollowingDetails(Table table)
    {
        try
        {
            var data = table.CreateInstance<CreateNpcData>();
            var request = new UpdateNpcRequest
            {
                CampaignId = _currentCampaignId,
                Name = data.Name,
                Description = data.Description,
                Race = data.Race,
                Class = data.Class,
                Faction = data.Faction
            };

            _operationSuccess = await _npcLogic.UpdateNpcAsync(_createdNpcId != 0 ? _createdNpcId : 1, _currentAccountId, request);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
            _operationSuccess = false;
        }
    }

    [When(@"I delete the NPC")]
    public async Task WhenIDeleteTheNpc()
    {
        try
        {
            _operationSuccess = await _npcLogic.DeleteNpcAsync(1);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
            _operationSuccess = false;
        }
    }

    [When(@"I retrieve all NPCs for campaign (.*)")]
    public async Task WhenIRetrieveAllNpcsForCampaign(int campaignId)
    {
        try
        {
            _retrievedNpcs = await _npcLogic.GetNpcList(_currentAccountId, campaignId);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [When(@"I retrieve the NPC with ID (.*)")]
    public async Task WhenIRetrieveTheNpcWithId(int npcId)
    {
        try
        {
            _retrievedNpc = await _npcLogic.GetNpc(npcId);
        }
        catch (Exception ex)
        {
            _caughtException = ex;
        }
    }

    [Then(@"the NPC should be created successfully")]
    public void ThenTheNpcShouldBeCreatedSuccessfully()
    {
        Assert.True(_operationSuccess, "NPC creation should succeed");
        Assert.True(_createdNpcId > 0, "Created NPC should have a valid ID");
    }

    [Then(@"the NPC should have name ""(.*)""")]
    public async Task ThenTheNpcShouldHaveName(string expectedName)
    {
        var npc = await _npcLogic.GetNpc(_createdNpcId != 0 ? _createdNpcId : 1);
        Assert.NotNull(npc);
        Assert.Equal(expectedName, npc.Name);
    }

    [Then(@"the NPC should have race ""(.*)""")]
    public async Task ThenTheNpcShouldHaveRace(string expectedRace)
    {
        var npc = await _npcLogic.GetNpc(_createdNpcId != 0 ? _createdNpcId : 1);
        Assert.NotNull(npc);
        Assert.Equal(expectedRace, npc.Stats?.Lineage);
    }

    [Then(@"the NPC should have class ""(.*)""")]
    public async Task ThenTheNpcShouldHaveClass(string expectedClass)
    {
        var npc = await _npcLogic.GetNpc(_createdNpcId != 0 ? _createdNpcId : 1);
        Assert.NotNull(npc);
        Assert.Equal(expectedClass, npc.Stats?.Occupation);
    }

    [Then(@"the creation should fail with ""(.*)""")]
    public void ThenTheCreationShouldFailWith(string expectedMessage)
    {
        Assert.False(_operationSuccess, "Operation should have failed");
        Assert.NotNull(_caughtException);
        Assert.Contains(expectedMessage, _caughtException.Message);
    }

    [Then(@"the creation should fail with unauthorized access")]
    public void ThenTheCreationShouldFailWithUnauthorizedAccess()
    {
        Assert.False(_operationSuccess, "Operation should have failed");
        Assert.NotNull(_caughtException);
        Assert.IsType<UnauthorizedAccessException>(_caughtException);
    }

    [Then(@"the NPC should be updated successfully")]
    public void ThenTheNpcShouldBeUpdatedSuccessfully()
    {
        Assert.True(_operationSuccess, "NPC update should succeed");
    }

    [Then(@"the NPC should be deleted successfully")]
    public void ThenTheNpcShouldBeDeletedSuccessfully()
    {
        Assert.True(_operationSuccess, "NPC deletion should succeed");
    }

    [Then(@"I should get (.*) NPCs")]
    public void ThenIShouldGetNpcs(int expectedCount)
    {
        Assert.NotNull(_retrievedNpcs);
        Assert.Equal(expectedCount, _retrievedNpcs.Count());
    }

    [Then(@"the NPC list should contain ""(.*)""")]
    public void ThenTheNpcListShouldContain(string npcName)
    {
        Assert.NotNull(_retrievedNpcs);
        Assert.Contains(_retrievedNpcs, npc => npc.Name == npcName);
    }

    [Then(@"I should get an NPC named ""(.*)""")]
    public void ThenIShouldGetAnNpcNamed(string expectedName)
    {
        Assert.NotNull(_retrievedNpc);
        Assert.Equal(expectedName, _retrievedNpc.Name);
    }

    private class CreateNpcData
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Race { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string? Faction { get; set; }
    }
}
