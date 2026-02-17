using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Models.Pcs;

namespace GM_Buddy.Business.Mappers;

/// <summary>
/// Mapper for converting between PC entity and DTO objects
/// </summary>
public class PcMapper
{
    /// <summary>
    /// Maps a PC entity to a PC DTO (excluding sensitive fields like account_id)
    /// </summary>
    public static PcDto MapToPcDto(Pc pc)
    {
        return new PcDto
        {
            PcId = pc.pc_id,
            Name = pc.name,
            Description = pc.description,
            CreatedAt = pc.created_at,
            UpdatedAt = pc.updated_at
        };
    }

    /// <summary>
    /// Maps a CreatePcRequest to a PC entity
    /// </summary>
    public static Pc MapToEntity(CreatePcRequest request, int accountId)
    {
        return new Pc
        {
            account_id = accountId,
            name = request.Name,
            description = request.Description
        };
    }

    /// <summary>
    /// Maps an UpdatePcRequest to a PC entity
    /// </summary>
    public static void MapToEntity(UpdatePcRequest request, Pc existingPc)
    {
        existingPc.name = request.Name;
        existingPc.description = request.Description;
    }
}
