namespace GM_Buddy.Contracts.Interfaces;

public interface INewAccountDataSeeder
{
    Task SeedDefaultDataForNewAccountAsync(int accountId);
}