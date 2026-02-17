using System.Text.Json.Serialization;

namespace GM_Buddy.Contracts.Models.Npcs.Dnd;

public class DnDStats
{    
    [JsonPropertyName("lineage")]
    public string? Lineage { get; set; }

    [JsonPropertyName("occupation")]
    public string? Occupation { get; set; }
}
