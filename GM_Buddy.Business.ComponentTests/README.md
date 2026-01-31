# GM_Buddy Business Component Tests

This project contains SpecFlow-based component tests for the GM_Buddy business logic layer.

## Overview

The component tests focus on testing business logic in isolation by mocking out database dependencies. These tests use SpecFlow (BDD framework) to write human-readable test scenarios that validate the behavior of the business layer.

## Technology Stack

- **SpecFlow 3.9.74** - BDD testing framework for .NET
- **xUnit** - Test runner
- **.NET 9.0** - Target framework
- **Fake Repositories** - In-memory implementations for testing without database dependencies

## Project Structure

```
GM_Buddy.Business.ComponentTests/
├── Features/               # SpecFlow feature files (.feature)
│   └── NpcManagement.feature
├── StepDefinitions/        # Step definition implementations
│   └── NpcManagementStepDefinitions.cs
├── Fakes/                  # Fake repository implementations
│   ├── FakeCampaignRepository.cs
│   └── FakeNpcRepository.cs
└── GM_Buddy.Business.ComponentTests.csproj
```

## Features Tested

### NPC Management

The `NpcManagement.feature` file contains scenarios testing the following capabilities:

1. **Creating NPCs** - Validates NPC creation with proper data and campaign association
2. **Validation** - Ensures proper error handling for:
   - Non-existent campaigns
   - Unauthorized access to campaigns from different accounts
3. **Updating NPCs** - Tests NPC updates including race/species, class, and other attributes
4. **Deleting NPCs** - Validates successful NPC deletion
5. **Querying NPCs** - Tests retrieval of NPCs by ID and by campaign

## Running the Tests

### From Command Line

```bash
# Navigate to the test project directory
cd GM_Buddy.Business.ComponentTests

# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test by name
dotnet test --filter "FullyQualifiedName~CreatingANewNPCWithValidData"
```

### From Visual Studio

1. Open the solution in Visual Studio
2. Open Test Explorer (Test > Test Explorer)
3. Click "Run All" to run all tests
4. Click individual tests to run specific scenarios

## Writing New Tests

### 1. Create a Feature File

Add a new `.feature` file in the `Features/` directory:

```gherkin
Feature: My New Feature
    As a user
    I want to do something
    So that I achieve a goal

Scenario: My test scenario
    Given some precondition
    When I perform an action
    Then I expect a result
```

### 2. Create Step Definitions

Add step definition methods in a class in the `StepDefinitions/` directory:

```csharp
[Binding]
public class MyFeatureStepDefinitions
{
    [Given(@"some precondition")]
    public void GivenSomePrecondition()
    {
        // Setup code
    }

    [When(@"I perform an action")]
    public void WhenIPerformAnAction()
    {
        // Action code
    }

    [Then(@"I expect a result")]
    public void ThenIExpectAResult()
    {
        // Assertion code
    }
}
```

### 3. Build the Project

When you build the project, SpecFlow automatically generates test code from your `.feature` files.

## Fake Repositories

The tests use in-memory fake repositories instead of actual database connections:

- **FakeCampaignRepository** - Stores campaigns in memory
- **FakeNpcRepository** - Stores NPCs in memory

These fakes implement the same interfaces as the real repositories but store data in `Dictionary<int, T>` collections, making tests fast and isolated.

## Benefits of Component Testing with SpecFlow

1. **Readable Tests** - Feature files use natural language (Gherkin syntax)
2. **Business Logic Focus** - Tests validate business rules without database overhead
3. **Fast Execution** - No database I/O means tests run quickly
4. **Isolated** - Each test scenario runs independently
5. **Living Documentation** - Feature files serve as executable specifications

## Example Scenario

```gherkin
Scenario: Creating a new NPC with valid data
    Given I have a valid campaign with ID 1 for account 100
    When I create an NPC with the following details:
        | Field       | Value                |
        | Name        | Gandalf              |
        | Description | A wise wizard        |
        | Race        | Human                |
        | Class       | Wizard               |
        | Faction     | The White Council    |
    Then the NPC should be created successfully
    And the NPC should have name "Gandalf"
    And the NPC should have race "Human"
    And the NPC should have class "Wizard"
```

This scenario validates that:
- NPCs can be created with all required fields
- The data is properly stored
- The NPC belongs to the correct campaign and account

## Maintenance

When adding new business logic:

1. Write a feature file describing the behavior
2. Implement step definitions
3. Add any necessary fake repositories or mock objects
4. Run tests to verify the implementation

## Dependencies

The component tests have project references to:
- `GM_Buddy.Business` - The business logic being tested
- `GM_Buddy.Contracts` - Shared contracts and interfaces

No reference to `GM_Buddy.Data` is needed since database calls are faked out.
