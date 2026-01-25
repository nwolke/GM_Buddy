
namespace GM_Buddy.Server.Helpers;

public interface IAuthHelper
{
    Task<int> GetAuthenticatedAccountIdAsync();
}