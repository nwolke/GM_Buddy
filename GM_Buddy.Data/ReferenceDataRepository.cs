using Dapper;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using System.Data;

namespace GM_Buddy.Data;

public class ReferenceDataRepository : IReferenceDataRepository
{
    private readonly IDbConnector _dbConnector;

    public ReferenceDataRepository(IDbConnector dbConnector)
    {
        _dbConnector = dbConnector;
    }

    // Race methods
    public async Task<IEnumerable<ReferenceRace>> GetRacesAsync(int gameSystemId, int? accountId = null, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT race_id, game_system_id, account_id, name, description, is_active, created_at, updated_at
            FROM reference_race
            WHERE game_system_id = @GameSystemId 
              AND is_active = true 
              AND (account_id IS NULL OR account_id = @AccountId)
            ORDER BY account_id NULLS FIRST, name";
        
        var cmd = new CommandDefinition(sql, new { GameSystemId = gameSystemId, AccountId = accountId }, cancellationToken: ct);
        return await dbConnection.QueryAsync<ReferenceRace>(cmd);
    }

    public async Task<ReferenceRace?> GetRaceByIdAsync(int raceId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT race_id, game_system_id, account_id, name, description, is_active, created_at, updated_at
            FROM reference_race
            WHERE race_id = @RaceId";
        
        var cmd = new CommandDefinition(sql, new { RaceId = raceId }, cancellationToken: ct);
        return await dbConnection.QueryFirstOrDefaultAsync<ReferenceRace>(cmd);
    }

    public async Task<int> CreateRaceAsync(ReferenceRace race, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            INSERT INTO reference_race (game_system_id, account_id, name, description, is_active)
            VALUES (@game_system_id, @account_id, @name, @description, @is_active)
            RETURNING race_id";
        
        var cmd = new CommandDefinition(sql, race, cancellationToken: ct);
        return await dbConnection.ExecuteScalarAsync<int>(cmd);
    }

    public async Task<bool> UpdateRaceAsync(ReferenceRace race, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            UPDATE reference_race
            SET name = @name,
                description = @description,
                is_active = @is_active,
                updated_at = NOW()
            WHERE race_id = @race_id AND account_id = @account_id";
        
        var cmd = new CommandDefinition(sql, race, cancellationToken: ct);
        int rowsAffected = await dbConnection.ExecuteAsync(cmd);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteRaceAsync(int raceId, int accountId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            DELETE FROM reference_race 
            WHERE race_id = @RaceId AND account_id = @AccountId";
        
        var cmd = new CommandDefinition(sql, new { RaceId = raceId, AccountId = accountId }, cancellationToken: ct);
        int rowsAffected = await dbConnection.ExecuteAsync(cmd);
        return rowsAffected > 0;
    }

    // Class methods
    public async Task<IEnumerable<ReferenceClass>> GetClassesAsync(int gameSystemId, int? accountId = null, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT class_id, game_system_id, account_id, name, description, is_active, created_at, updated_at
            FROM reference_class
            WHERE game_system_id = @GameSystemId 
              AND is_active = true 
              AND (account_id IS NULL OR account_id = @AccountId)
            ORDER BY account_id NULLS FIRST, name";
        
        var cmd = new CommandDefinition(sql, new { GameSystemId = gameSystemId, AccountId = accountId }, cancellationToken: ct);
        return await dbConnection.QueryAsync<ReferenceClass>(cmd);
    }

    public async Task<ReferenceClass?> GetClassByIdAsync(int classId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            SELECT class_id, game_system_id, account_id, name, description, is_active, created_at, updated_at
            FROM reference_class
            WHERE class_id = @ClassId";
        
        var cmd = new CommandDefinition(sql, new { ClassId = classId }, cancellationToken: ct);
        return await dbConnection.QueryFirstOrDefaultAsync<ReferenceClass>(cmd);
    }

    public async Task<int> CreateClassAsync(ReferenceClass referenceClass, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            INSERT INTO reference_class (game_system_id, account_id, name, description, is_active)
            VALUES (@game_system_id, @account_id, @name, @description, @is_active)
            RETURNING class_id";
        
        var cmd = new CommandDefinition(sql, referenceClass, cancellationToken: ct);
        return await dbConnection.ExecuteScalarAsync<int>(cmd);
    }

    public async Task<bool> UpdateClassAsync(ReferenceClass referenceClass, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            UPDATE reference_class
            SET name = @name,
                description = @description,
                is_active = @is_active,
                updated_at = NOW()
            WHERE class_id = @class_id AND account_id = @account_id";
        
        var cmd = new CommandDefinition(sql, referenceClass, cancellationToken: ct);
        int rowsAffected = await dbConnection.ExecuteAsync(cmd);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteClassAsync(int classId, int accountId, CancellationToken ct = default)
    {
        using IDbConnection dbConnection = _dbConnector.CreateConnection();
        const string sql = @"
            DELETE FROM reference_class 
            WHERE class_id = @ClassId AND account_id = @AccountId";
        
        var cmd = new CommandDefinition(sql, new { ClassId = classId, AccountId = accountId }, cancellationToken: ct);
        int rowsAffected = await dbConnection.ExecuteAsync(cmd);
        return rowsAffected > 0;
    }
}
