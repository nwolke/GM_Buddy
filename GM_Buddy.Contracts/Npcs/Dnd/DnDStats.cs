﻿using System.Text.Json.Serialization;

namespace GM_Buddy.Contracts.Npcs.Dnd;

public class DnDStats
{
    [JsonPropertyName("attributes")]
    public required DndAttributes Attributes { get; set; }
    [JsonPropertyName("languages")]
    public required string[] Languages { get; set; }
}

public class DndAttributes
{
    [JsonPropertyName("strength")]
    public int Strength { get; set; }
    [JsonPropertyName("dexterity")]
    public int Dexterity { get; set; }
    [JsonPropertyName("constitution")]
    public int Constitution { get; set; }
    [JsonPropertyName("intelligence")]
    public int Intelligence { get; set; }
    [JsonPropertyName("wisdom")]
    public int Wisdom { get; set; }
    [JsonPropertyName("charisma")]
    public int Charisma { get; set; }
}
