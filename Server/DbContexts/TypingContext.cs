using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SpeedwayTyperApp.Server.Models;

namespace SpeedwayTyperApp.Server.DbContexts
{
    public class TypingContext : IdentityDbContext<User>
    {
        public DbSet<Team> Teams { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Prediction> Predictions { get; set; }

        public TypingContext(DbContextOptions<TypingContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.HostTeam)
                .WithMany()
                .HasForeignKey(m => m.HostTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.GuestTeam)
                .WithMany()
                .HasForeignKey(m => m.GuestTeamId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
