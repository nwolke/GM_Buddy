using GM_Buddy.Contracts.DbEntities;

namespace GM_Buddy.Contracts.Interfaces;

public interface IAuthLogic
{
    Task<Account> GetOrCreateAccountByCognitoSubAsync(string cognitoSub, string email);
}