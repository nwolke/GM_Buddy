using System.ComponentModel.DataAnnotations;

namespace GM_Buddy.Contracts.AuthModels.Entities;
public class Client
{
    [Key]
    public int Id { get; set; }
    // Unique identifier for the client application.
    [Required]
    [MaxLength(100)]
    public required string Client_Id { get; set; }
    // Name of the client application.
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
    // URL for the client application.
    [Required]
    [MaxLength(200)]
    public required string Client_URL { get; set; }
}
