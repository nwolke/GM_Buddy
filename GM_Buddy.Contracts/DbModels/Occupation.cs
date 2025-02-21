using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace GM_Buddy.Contracts.DbModels
{
    [Table("occupation")]
    [PrimaryKey("occupation_id")]
    public class Occupation
    {
        public int occupation_id { get; set; }
        public required string occupation_name { get; set; }
    }
}
