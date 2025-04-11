namespace GM_Buddy.Contracts.AuthModels.DbModels;
public record Role
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
}
