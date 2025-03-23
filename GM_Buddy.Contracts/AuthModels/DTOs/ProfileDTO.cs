namespace GM_Buddy.Contracts.AuthModels.DTOs;

public class ProfileDTO
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public required List<string> Roles { get; set; }
}
