using GM_Buddy.Contracts.Interfaces;
using Microsoft.Extensions.Logging;

namespace GM_Buddy.Business.Helpers;

public class GameSystemHelper
{
    private readonly IGameSystemRepository _gameSystemRepository;
    private readonly ILogger _logger;

    private const string DefaultGameSystemName = "Dungeons & Dragons (5e)";

    public GameSystemHelper(
        IGameSystemRepository gameSystemRepository,
        ILogger logger)
    {
        _gameSystemRepository = gameSystemRepository;
        _logger = logger;
    }

    /// <summary>
    /// Resolves a game system ID from the request system name, falling back to the default system.
    /// </summary>
    /// <param name="requestedSystemName">The system name from the request (may be null or empty)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The resolved game system ID</returns>
    /// <exception cref="InvalidOperationException">Thrown when the default game system is not found in the database</exception>
    public async Task<int> ResolveGameSystemIdAsync(string? requestedSystemName, CancellationToken ct = default)
    {
        // Try to use the requested system if provided
        if (!string.IsNullOrWhiteSpace(requestedSystemName))
        {
            var gameSystem = await _gameSystemRepository.GetByNameAsync(requestedSystemName, ct);
            if (gameSystem != null)
            {
                _logger.LogInformation("Using game system: {SystemName} (ID: {SystemId})", 
                    requestedSystemName, gameSystem.game_system_id);
                return gameSystem.game_system_id;
            }

            _logger.LogWarning("Game system '{SystemName}' not found, using default", requestedSystemName);
        }

        // Fall back to default system
        var defaultSystem = await _gameSystemRepository.GetByNameAsync(DefaultGameSystemName, ct);
        if (defaultSystem == null)
        {
            var errorMsg = $"Default game system '{DefaultGameSystemName}' not found in database. Please ensure init.sql has been run.";
            _logger.LogError(errorMsg);
            throw new InvalidOperationException(errorMsg);
        }

        _logger.LogInformation("Using default game system: {SystemName} (ID: {SystemId})", 
            DefaultGameSystemName, defaultSystem.game_system_id);
        return defaultSystem.game_system_id;
    }
}
