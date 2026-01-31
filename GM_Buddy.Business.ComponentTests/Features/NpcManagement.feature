Feature: NPC Management
    As a Game Master
    I want to manage NPCs in my campaigns
    So that I can track characters and their details

Scenario: Creating a new NPC with valid data
    Given I have a valid campaign with ID 1 for account 100
    When I create an NPC with the following details:
        | Field       | Value                |
        | Name        | Aldric               |
        | Description | A wise wizard        |
        | Race        | Human                |
        | Class       | Wizard               |
        | Faction     | The Mage Guild       |
    Then the NPC should be created successfully
    And the NPC should have name "Aldric"
    And the NPC should have race "Human"
    And the NPC should have class "Wizard"

Scenario: Creating NPC fails when campaign does not exist
    Given no campaign exists with ID 999
    When I attempt to create an NPC for campaign 999
    Then the creation should fail with "Campaign with ID 999 not found"

Scenario: Creating NPC fails when campaign belongs to different account
    Given I have a campaign with ID 2 belonging to account 200
    When I attempt to create an NPC for campaign 2 with account 100
    Then the creation should fail with unauthorized access

Scenario: Updating an existing NPC
    Given I have an existing NPC with ID 1 for account 100
    When I update the NPC with the following details:
        | Field       | Value                |
        | Name        | Aldric the Wise      |
        | Description | The Sage Mentor      |
        | Race        | Celestial            |
        | Class       | Wizard               |
    Then the NPC should be updated successfully
    And the NPC should have name "Aldric the Wise"
    And the NPC should have race "Celestial"

Scenario: Deleting an NPC
    Given I have an existing NPC with ID 1
    When I delete the NPC
    Then the NPC should be deleted successfully

Scenario: Getting NPCs for a specific campaign
    Given I have a campaign with ID 1 for account 100
    And the campaign has the following NPCs:
        | Name    | Race     | Class   |
        | Beren   | Halfling | Rogue   |
        | Theron  | Human    | Ranger  |
        | Elara   | Elf      | Archer  |
    When I retrieve all NPCs for campaign 1
    Then I should get 3 NPCs
    And the NPC list should contain "Beren"
    And the NPC list should contain "Theron"
    And the NPC list should contain "Elara"

Scenario: Getting a specific NPC by ID
    Given I have an existing NPC with ID 1 named "Thorin"
    When I retrieve the NPC with ID 1
    Then I should get an NPC named "Thorin"
