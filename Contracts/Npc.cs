using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace GM_Buddy.Server.DbModels
{
    [PrimaryKey("npc_id")]
    [Table("npc")]
    public class Npc
    {
        public int npc_id { get; set; }
        public required Account account { get; set; }
        public required Game_System game_system { get; set; }
        public required string name { get; set; }
        public required Lineage lineage { get; set; }
        public required Occupation occupation { get; set; }
        [Column(TypeName = "jsonb")]
        public string? stats { get; set; }
        public string? description { get; set; }
        public string? gender { get; set; }
    }
}