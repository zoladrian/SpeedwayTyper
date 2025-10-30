using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.DbContexts
{
    public class TypingContext : IdentityDbContext<UserModel>
    {
        public DbSet<TeamModel> Teams { get; set; }
        public DbSet<MatchModel> Matches { get; set; }
        public DbSet<PredictionModel> Predictions { get; set; }
        public DbSet<InvitationModel> Invitations { get; set; }
        public DbSet<InviteCodeModel> InviteCodes { get; set; }

        public TypingContext(DbContextOptions<TypingContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            modelBuilder.Entity<InviteCodeModel>()
                .HasIndex(i => i.Code)
                .IsUnique();

            modelBuilder.Entity<InviteCodeModel>()
                .HasOne<UserModel>()
                .WithMany()
                .HasForeignKey(i => i.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
