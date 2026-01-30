namespace GM_Buddy.Contracts.AuthModels.Entities;
public record Role
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
}
