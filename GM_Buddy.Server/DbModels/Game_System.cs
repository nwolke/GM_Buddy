using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace GM_Buddy.Server.DbModels
{
    [Table("game_system")]
    [PrimaryKey("game_system_id")]
    public class Game_System
    {
        public int game_system_id { get; set; }
        public required string game_system_name { get; set; }
    }
}
