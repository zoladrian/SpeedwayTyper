using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.DbContexts
{
    public class TypingContext : IdentityDbContext<UserModel>
    {
        public DbSet<TeamModel> Teams { get; set; } = null!;
        public DbSet<SeasonModel> Seasons { get; set; } = null!;
        public DbSet<RoundModel> Rounds { get; set; } = null!;
        public DbSet<MatchModel> Matches { get; set; } = null!;
        public DbSet<PredictionModel> Predictions { get; set; } = null!;

        public TypingContext(DbContextOptions<TypingContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RoundModel>()
                .HasOne(r => r.Season)
                .WithMany()
                .HasForeignKey(r => r.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MatchModel>()
                .HasOne(m => m.Season)
                .WithMany()
                .HasForeignKey(m => m.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MatchModel>()
                .HasOne(m => m.Round)
                .WithMany()
                .HasForeignKey(m => m.RoundId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MatchModel>()
                .HasOne(m => m.HostTeam)
                .WithMany()
                .HasForeignKey(m => m.HostTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MatchModel>()
                .HasOne(m => m.GuestTeam)
                .WithMany()
                .HasForeignKey(m => m.GuestTeamId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
