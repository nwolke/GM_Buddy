using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.Models.Pcs;

namespace GM_Buddy.Business.Mappers;

public class PcMapper
{
    public static PcDto MapToPcDto(Pc pc) => new PcDto
    {
        Pc_Id = pc.pc_id,
        Name = pc.name,
        Description = pc.description
    };
}
