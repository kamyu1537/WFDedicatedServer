using Microsoft.EntityFrameworkCore;
using WFDS.Database.DbSet;

namespace WFDS.Database;

public class DataDbContext(DbContextOptions<DataDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players { get; set; }
    public DbSet<BannedPlayer> BannedPlayers { get; set; } 
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>().ToTable("players");
        modelBuilder.Entity<BannedPlayer>().ToTable("banned_players");

        base.OnModelCreating(modelBuilder);
    }
}