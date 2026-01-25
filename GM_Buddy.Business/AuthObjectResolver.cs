using GM_Buddy.Contracts.AuthModels.Entities;
using GM_Buddy.Contracts.AuthModels.Responses;
using GM_Buddy.Contracts.Interfaces;

namespace GM_Buddy.Business;
public class AuthObjectResolver : IAuthObjectResolver
{
    private readonly IAuthRepository _authRepository;

    public AuthObjectResolver(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<ProfileResponse?> GetUserProfile(string email)
    {
        User? user = await _authRepository.GetUserByEmail(email);
        if (user == null)
        {
            return null;
        }
        var roles = await _authRepository.GetAllUserRoles(user.Id);
        ProfileResponse profile = new()
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
