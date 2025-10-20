using Dapper;
using GM_Buddy.Contracts.AuthModels.Entities;
using GM_Buddy.Contracts.Interfaces;
using System.Data;

namespace GM_Buddy.Data;

public class AuthRepository : IAuthRepository
{
    private readonly IDbConnector _dbConnector;

    public AuthRepository(IDbConnector dbConnector)
    {
        _dbConnector = dbConnector;
    }

    #region SigningKey
    public async Task<SigningKey?> GetActiveSigningKeyAsync()
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"SELECT id, key_id as Key_Id, private_key as Private_Key, public_key as Public_Key, is_active as Is_Active, created_at as Created_At, expires_at as Expires_At
                             FROM auth.signing_key WHERE is_active = true LIMIT 1";
        return await dbConnection.QueryFirstOrDefaultAsync<SigningKey>(sql);
    }

    public async Task<IEnumerable<SigningKey?>> GetAllActiveSigningKeyAsync()
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"SELECT id, key_id as Key_Id, private_key as Private_Key, public_key as Public_Key, is_active as Is_Active, created_at as Created_At, expires_at as Expires_At
                             FROM auth.signing_key WHERE is_active = true";
        return await dbConnection.QueryAsync<SigningKey>(sql);
    }

    public async Task DeactiveSigningKey()
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = "UPDATE auth.signing_key SET is_active = false WHERE is_active = true";
        _ = await dbConnection.ExecuteAsync(sql);
    }

    public async Task InsertSigningKey(SigningKey signingKey)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"INSERT INTO auth.signing_key (key_id, private_key, public_key, is_active, created_at, expires_at)
                             VALUES (@KeyId, @PrivateKey, @PublicKey, @IsActive, @CreatedAt, @ExpiresAt)";
        var parameters = new
        {
            KeyId = signingKey.Key_Id,
            PrivateKey = signingKey.Private_Key,
            PublicKey = signingKey.Public_Key,
            IsActive = signingKey.Is_Active,
            CreatedAt = signingKey.Created_At,
            ExpiresAt = signingKey.Expires_At
        };
        await dbConnection.ExecuteAsync(sql, parameters);
    }
    #endregion

    #region User
    public async Task<int> InsertNewUser(User user)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"INSERT INTO auth.user (first_name, last_name, email, password, salt)
                             VALUES (@FirstName, @LastName, @Email, @Password, @Salt)
                             RETURNING id";
        var parameters = new
        {
            FirstName = user.First_Name,
            LastName = user.Last_Name,
            Email = user.Email,
            Password = user.Password,
            Salt = user.Salt
        };
        return await dbConnection.ExecuteScalarAsync<int>(sql, parameters);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"SELECT id, first_name as First_Name, last_name as Last_Name, email, password, salt
                             FROM auth.user WHERE email = @Email LIMIT 1";
        return await dbConnection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task UpdateUser(User user)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"UPDATE auth.user
                             SET first_name = @FirstName, last_name = @LastName, email = @Email, password = @Password
                             WHERE id = @Id";
        var parameters = new
        {
            FirstName = user.First_Name,
            LastName = user.Last_Name,
            Email = user.Email,
            Password = user.Password,
            Id = user.Id
        };
        await dbConnection.ExecuteAsync(sql, parameters);
    }
    #endregion

    #region Role
    public async Task<Role?> GetRole(string name)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = "SELECT id, name, description FROM auth.role WHERE name = @Name LIMIT 1";
        return await dbConnection.QueryFirstOrDefaultAsync<Role>(sql, new { Name = name });
    }

    public async Task<IEnumerable<Role>> GetAllRoles()
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = "SELECT id, name, description FROM auth.role";
        return await dbConnection.QueryAsync<Role>(sql);
    }

    public async Task<IEnumerable<Role>> GetAllUserRoles(int userId)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"SELECT r.id, r.name, r.description
                             FROM auth.user_role ur
                             JOIN auth.role r ON ur.role_id = r.id
                             WHERE ur.user_id = @UserId";
        return await dbConnection.QueryAsync<Role>(sql, new { UserId = userId });
    }
    #endregion

    #region UserRole
    public async Task InsertUserRole(int userId, int roleId)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = "INSERT INTO auth.user_role (user_id, role_id) VALUES (@UserId, @RoleId)";
        _ = await dbConnection.ExecuteAsync(sql, new { UserId = userId, RoleId = roleId });
    }
    #endregion

    #region Client
    public async Task<Client?> GetClient(string clientId)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = "SELECT id, client_id as Client_Id, name, client_url as Client_URL FROM auth.client WHERE client_id = @ClientId LIMIT 1";
        return await dbConnection.QueryFirstOrDefaultAsync<Client>(sql, new { ClientId = clientId });
    }
    #endregion
}
