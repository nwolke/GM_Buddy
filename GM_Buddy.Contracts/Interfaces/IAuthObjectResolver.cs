using GM_Buddy.Contracts.AuthModels.Responses;

namespace GM_Buddy.Contracts.Interfaces;
public interface IAuthObjectResolver
{
    Task<ProfileResponse?> GetUserProfile(string email);
}