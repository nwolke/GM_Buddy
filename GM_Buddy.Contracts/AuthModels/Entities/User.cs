namespace GM_Buddy.Contracts.AuthModels.Entities;

public record User
{
    public int Id { get; set; }
    public required string First_Name { get; set; }
    public string? Last_Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Salt { get; set; }
}
