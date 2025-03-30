using GM_Buddy.Contracts.AuthModels.DbModels;

namespace GM_Buddy.Contracts.Interfaces
{
    public interface IAuthRepository
    {
        Task DeactiveSigningKey();
        Task<SigningKey?> GetActiveSigningKeyAsync();
        Task<IEnumerable<SigningKey?>> GetAllActiveSigningKeyAsync();
        Task InsertSigningKey(SigningKey signingKey);
        Task<int> InsertNewUser(User user);
        Task<User?> GetUserByEmail(string email);
        Task<Role?> GetRole(string name);
        Task<IEnumerable<Role>> GetAllRoles();
        Task<IEnumerable<Role>> GetAllUserRoles(int userId);
        Task InsertUserRole(int userId, int roleId);
        Task UpdateUser(User user);
        Task<Client?> GetClient(string clientId);
    }
}