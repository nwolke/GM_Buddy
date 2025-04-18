using Dapper;
using GM_Buddy.Contracts.AuthModels.Entities;
using GM_Buddy.Contracts.Interfaces;
using System.Data;

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

    public async Task<IEnumerable<SigningKey?>> GetAllActiveSigningKeyAsync()
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        IEnumerable<SigningKey> signingKey = await dbConnection.QueryAsync<SigningKey>(
            "SELECT * FROM auth.signing_key WHERE is_active = true");
        return signingKey;
    }

    public async Task DeactiveSigningKey()
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        _ = await dbConnection.ExecuteAsync("UPDATE auth.signing_key SET is_active = false WHERE is_active = true");
    }

    public async Task InsertSigningKey(SigningKey signingKey)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        var sproc = $"INSERT INTO auth.signing_key (key_id, private_key, public_key, is_active, created_at, expires_at) " +
            $"VALUES ('{signingKey.Key_Id}', '{signingKey.Private_Key}', '{signingKey.Public_Key}', {signingKey.Is_Active}, '{signingKey.Created_At}', '{signingKey.Expires_At}' )";
        await dbConnection.ExecuteAsync(sproc);
    }
    #endregion

    #region User
    public async Task<int> InsertNewUser(User user)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        var sproc = $"INSERT INTO auth.user (first_name, last_name, email, password, salt) " +
            $"VALUES ('{user.First_Name}', '{user.Last_Name}', '{user.Email}', '{user.Password}', '{user.Salt}' ) " +
            $"RETURNING id";
        return await dbConnection.ExecuteScalarAsync<int>(sproc);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        var user = await dbConnection.QueryFirstOrDefaultAsync<User>(
            $"SELECT * FROM auth.user WHERE email = '{email}'");
        return user;
    }

    public async Task UpdateUser(User user)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        var sproc = $"UPDATE auth.user SET first_name = '{user.First_Name}', last_name = '{user.Last_Name}', email = '{user.Email}', password = '{user.Password}' " +
            $"WHERE id = {user.Id}";
        await dbConnection.ExecuteAsync(sproc);
    }

    #endregion

    #region Role

    public async Task<Role?> GetRole(string name)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        string sproc = $"SELECT id, name, description FROM auth.role WHERE name = '{name}' LIMIT 1";
        return await dbConnection.QueryFirstOrDefaultAsync<Role>(sproc);
    }

    public async Task<IEnumerable<Role>> GetAllRoles()
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        string sproc = $"SELECT id, name, description FROM auth.role";
        return await dbConnection.QueryAsync<Role>(sproc);
    }

    public async Task<IEnumerable<Role>> GetAllUserRoles(int userId)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        string sproc = $"SELECT r.* FROM auth.user_role ur " +
            $"JOIN auth.role r ON ur.role_id = r.id " +
            $"where ur.user_id = {userId}";
        return await dbConnection.QueryAsync<Role>(sproc);
    }

    #endregion

    #region UserRole

    public async Task InsertUserRole(int userId, int roleId)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        string sproc = $"INSERT INTO auth.user_role (user_id, role_id) " +
            $"VALUES ({userId}, {roleId})";
        _ = await dbConnection.ExecuteAsync(sproc);
    }

    #endregion

    #region Client

    public async Task<Client?> GetClient(string clientId)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        string sproc = $"SELECT * FROM auth.client WHERE client_id = '{clientId}' LIMIT 1";
        return await dbConnection.QueryFirstOrDefaultAsync<Client>(sproc);
    }

    #endregion
}
