using GM_Buddy.Contracts.Interfaces;
using System.Data;
using Dapper;
using GM_Buddy.Contracts.AuthModels.DbModels;

namespace GM_Buddy.Data;

public class AuthRepository : IAuthRepository
{
    public required IDbConnector _dbConnector;
    public AuthRepository(IDbConnector dbConnector)
    {
        _dbConnector = dbConnector;
    }

    #region SigningKey
    public async Task<SigningKey?> GetActiveSigningKeyAsync()
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        var signingKey = await dbConnection.QueryFirstOrDefaultAsync<SigningKey>(
            "SELECT * FROM auth.signing_key WHERE is_active = true LIMIT 1");
        return signingKey;
    }

    public async Task DeactiveSigningKey()
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        await dbConnection.ExecuteAsync("UPDATE auth.signing_key SET is_active = false WHERE is_active = true");
    }

    public async Task InsertSigningKey(SigningKey signingKey)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        var sproc = $"INSERT INTO auth.signing_key (key_id, private_key, public_key, is_active, created_at, expires_at) " +
            $"VALUES ('{signingKey.KeyId}', '{signingKey.PrivateKey}', '{signingKey.PublicKey}', {signingKey.IsActive}, '{signingKey.CreatedAt}', '{signingKey.ExpiresAt}' )";
        await dbConnection.ExecuteAsync(sproc);
    }
    #endregion

    #region User
    public async Task<int> InsertNewUser(User user)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        var sproc = $"INSERT INTO auth.users (first_name, last_name, email, password, salt) " +
            $"VALUES ('{user.FirstName}', '{user.LastName}', '{user.Email}', '{user.Password}', '{user.Salt}' ) " +
            $"RETURNING id";
        return await dbConnection.ExecuteAsync(sproc);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        var user = await dbConnection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM auth.users WHERE email = @Email", new { Email = email });
        return user;
    }

    public async Task UpdateUser(User user)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        var sproc = $"UPDATE auth.users SET first_name = '{user.FirstName}', last_name = '{user.LastName}', email = '{user.Email}', password = '{user.Password}' " +
            $"WHERE id = {user.Id}";
        await dbConnection.ExecuteAsync(sproc);
    }

    #endregion

    #region Role

    public async Task<Role?> GetRole(string name)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        var sproc = $"SELECT id, name, description FROM auth.roles WHERE name = {name} LIMIT 1";
        return await dbConnection.QueryFirstOrDefaultAsync<Role>(sproc);
    }

    public async Task<IEnumerable<Role>> GetAllRoles()
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        var sproc = $"SELECT id, name, description FROM auth.roles";
        return await dbConnection.QueryAsync<Role>(sproc);
    }

    public async Task<IEnumerable<Role>> GetAllUserRoles(int userId)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        var sproc = $"SELECT r FROM auth.user_role ur " + 
            $"JOIN auth.role r ON ur.role_id = r.id " +
            $"where ur.user_id = {userId}";
        return await dbConnection.QueryAsync<Role>(sproc);
    }

    #endregion

    #region UserRole

    public async Task InsertUserRole(int userId, int roleId)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        var sproc = $"INSERT INTO auth.user_roles (user_id, role_id) " +
            $"VALUES ({userId}, {roleId})";
        await dbConnection.ExecuteAsync(sproc);
    }

    #endregion

    #region Client

    public async Task<Client?> GetClient(string clientId)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        var sproc = $"SELECT * FROM auth.client WHERE id = '{clientId}' LIMIT 1";
        return await dbConnection.QueryFirstOrDefaultAsync<Client>(sproc);
    }

    #endregion
}
