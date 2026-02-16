using Dapper;
using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Interfaces;
using System.Data;

namespace GM_Buddy.Data;

/// <summary>
/// Repository for reference data
/// Note: Lineage and Occupation methods have been removed as part of GM-108
/// to simplify the MVP. The tables are removed from the database schema.
/// </summary>
public class ReferenceDataRepository : IReferenceDataRepository
{
    private readonly IDbConnector _dbConnector;

    public ReferenceDataRepository(IDbConnector dbConnector)
    {
        _dbConnector = dbConnector;
    }

    // All lineage and occupation methods removed - lineage is now freeform text in NPC stats
}
