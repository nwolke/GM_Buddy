using GM_Buddy.Contracts.AuthModels.DTOs;

namespace GM_Buddy.Contracts.Interfaces;
public interface IAuthObjectResolver
{
    Task<ProfileDTO?> GetUserProfile(string email);
}