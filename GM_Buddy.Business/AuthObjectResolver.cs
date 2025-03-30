using GM_Buddy.Contracts.AuthModels.DTOs;
using GM_Buddy.Contracts.Interfaces;

namespace GM_Buddy.Business;
public class AuthObjectResolver : IAuthObjectResolver
{
    private IAuthRepository _authRepository;

    public AuthObjectResolver(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<ProfileDTO?> GetUserProfile(string email)
    {
        var user = await _authRepository.GetUserByEmail(email);
        if (user == null)
        {
            return null;
        }
        var roles = await _authRepository.GetAllUserRoles(user.Id);
        var profile = new ProfileDTO
        {
            Id = user.Id,
            FirstName = user.First_Name,
            LastName = user.Last_Name,
            Email = user.Email,
            Roles = roles.Select(r => r.Name).ToList()
        };
        return profile;
    }
}
