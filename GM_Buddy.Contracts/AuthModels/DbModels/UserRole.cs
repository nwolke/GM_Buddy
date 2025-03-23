namespace GM_Buddy.Contracts.AuthModels.DbModels;
public record UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}
