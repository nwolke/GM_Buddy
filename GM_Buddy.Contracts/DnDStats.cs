namespace GM_Buddy.Contracts;

public class DnDStats
{
    public required Attributes Attributes { get; set; }
    public required string[] Languages { get; set; }
}

public class Attributes
{
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Constitution { get; set; }
    public int Intelligence { get; set; } 
    public int Wisdom { get; set; }
    public int Charisma { get; set; }
}
