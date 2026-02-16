using GM_Buddy.Contracts.DbEntities;

namespace GM_Buddy.Contracts.Interfaces;

/// <summary>
/// Repository interface for reference data
/// Note: Lineage and Occupation methods have been removed as part of GM-108
/// to simplify the MVP. The tables are removed from the database schema.
/// </summary>
public interface IReferenceDataRepository
{
    // Reference data repository methods removed - lineage is now freeform text
}
