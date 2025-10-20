namespace GM_Buddy.Contracts.DbEntities;

public class Occupation
{
    public int occupation_id { get; set; }
    public int game_system_id { get; set; }
    public required string occupation_name { get; set; }
}
