using GM_Buddy.Contracts.DbEntities;
using GM_Buddy.Contracts.DTOs;

namespace GM_Buddy.Business.Mappers;

public static class CampaignMappers
{
    public static Campaign CampaignToDbEntity(this CampaignDTO dto, int accountId)
    {
        return new Campaign
        {
            campaign_id = dto.Campaign_id,
            account_id = accountId,
            game_system_id = dto.Game_system_id,
            name = dto.Name,
            description = dto.Description
        };
    }

    public static CampaignDTO DbEntityToDto(this Campaign entity)
    {
        return new CampaignDTO
        {
            Campaign_id = entity.campaign_id,
            Name = entity.name,
            Description = entity.description,
            Game_system_id = entity.game_system_id,
            Game_system_name = entity.game_system_name
        };
    }

}
