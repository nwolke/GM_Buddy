using System.ComponentModel.DataAnnotations;

namespace GM_Buddy.Contracts.AuthModels.DbModels;
public class Client
{
    [Key]
    public int Id { get; set; }
    // Unique identifier for the client application.
    [Required]
    [MaxLength(100)]
    public required string ClientId { get; set; }
    // Name of the client application.
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
    // URL for the client application.
    [Required]
    [MaxLength(200)]
    public required string ClientURL { get; set; }
}
