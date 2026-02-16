using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using GM_Buddy.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GM_Buddy.Server.Controllers;

/// <summary>
/// Reference data controller
/// Note: All lineage and occupation endpoints have been removed as part of GM-108
/// to simplify the MVP. Lineage is now freeform text in NPC stats.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReferenceDataController : ControllerBase
{
    private readonly ILogger<ReferenceDataController> _logger;
    private readonly IReferenceDataRepository _repository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly IAuthHelper _authHelper;

    public ReferenceDataController(
        ILogger<ReferenceDataController> logger,
        IReferenceDataRepository repository,
        ICampaignRepository campaignRepository,
        IAuthHelper authHelper)
    {
        _logger = logger;
        _repository = repository;
        _campaignRepository = campaignRepository;
        _authHelper = authHelper;
    }

    // All lineage and occupation endpoints removed - lineage is now freeform text
}
