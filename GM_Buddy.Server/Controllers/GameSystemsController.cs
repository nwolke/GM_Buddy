using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameSystemsController : ControllerBase
{
    private readonly ILogger<GameSystemsController> _logger;
    private readonly IGameSystemRepository _repository;

    public GameSystemsController(
        ILogger<GameSystemsController> logger,
        IGameSystemRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    /// <summary>
    /// Get all available game systems
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Game_System>>> GetGameSystems()
    {
        try
        {
            var gameSystems = await _repository.GetAllAsync();
            return Ok(gameSystems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving game systems");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific game system by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Game_System>> GetGameSystem(int id)
    {
        try
        {
            var gameSystem = await _repository.GetByIdAsync(id);
            if (gameSystem == null)
            {
                return NotFound($"Game system with ID {id} not found");
            }
            return Ok(gameSystem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving game system {GameSystemId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
