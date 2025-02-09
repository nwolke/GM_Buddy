using GM_Buddy.Server.DbModels;
using Microsoft.EntityFrameworkCore;

namespace GM_Buddy.Server.DbContexts
{
    public class GmBuddyDbContext : DbContext
    {
        public GmBuddyDbContext(DbContextOptions<GmBuddyDbContext> options) :
            base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Npc>().HasOne(x => x.occupation);
            modelBuilder.Entity<Npc>().HasOne(x => x.lineage);
            modelBuilder.Entity<Npc>().HasOne(x => x.account);
            modelBuilder.Entity<Npc>().HasOne(x => x.game_system);
        }

        public DbSet<Npc> Npcs { get; set; }
        public DbSet<Occupation> Occupations { get; set; }
        public DbSet<Lineage> Lineages { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Game_System> GameSystems { get; set; }

        
    }
}
