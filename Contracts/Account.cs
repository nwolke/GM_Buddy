using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace GM_Buddy.Server.DbModels
{
    [Table("account")]
    [PrimaryKey("account_id")]
    public class Account
    {
        public int account_id { get; set; }
        public required string account_name { get; set; }
    }
}
