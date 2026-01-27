namespace GM_Buddy.Contracts.AuthModels.Entities;

public record SigningKey
{
    public int Id { get; set; }
    public required string Key_Id { get; set; }
    public required string Private_Key { get; set; }
    public required string Public_Key { get; set; }
    public bool Is_Active { get; set; }
    public DateTime Created_At { get; set; }
    public DateTime Expires_At { get; set; }
}
