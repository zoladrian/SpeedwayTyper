using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using SpeedwayTyperApp.Server.DbContexts;
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Server.Services;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Tests
{
    public class MatchServiceTests
    {
        private TypingContext _context = null!;
        private MatchService _matchService = null!;
        private PredictionService _predictionService = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<TypingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new TypingContext(options);

            var matchRepository = new MatchRepository(_context);
            var userRepository = new UserRepository(_context);
            var predictionRepository = new PredictionRepository(_context);

            _predictionService = new PredictionService(matchRepository, userRepository, predictionRepository);
            _matchService = new MatchService(_context, _predictionService);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task UpdateMatchAsync_WhenMatchCompleted_RecalculatesPredictionPointsAndUserStatistics()
        {
            var hostTeam = new TeamModel { TeamId = 1, Name = "Host" };
            var guestTeam = new TeamModel { TeamId = 2, Name = "Guest" };

            var match = new MatchModel
            {
                MatchId = 1,
                Date = DateTime.UtcNow,
                Round = 1,
                HostTeamId = hostTeam.TeamId,
                GuestTeamId = guestTeam.TeamId,
                IsCompleted = false
            };

            var user = new UserModel
            {
                Id = "user-1",
                UserName = "player",
                Email = "player@example.com",
                EmailConfirmed = true
            };

            var prediction = new PredictionModel
            {
                PredictionId = 1,
                MatchId = match.MatchId,
                UserId = user.Id,
                HostTeamPredictedScore = 45,
                GuestTeamPredictedScore = 45,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Teams.AddRangeAsync(hostTeam, guestTeam);
            await _context.Matches.AddAsync(match);
            await _context.Users.AddAsync(user);
            await _context.Predictions.AddAsync(prediction);
            await _context.SaveChangesAsync();

            match.HostTeamScore = 45;
            match.GuestTeamScore = 45;
            match.IsCompleted = true;

            await _matchService.UpdateMatchAsync(match);

            var updatedPrediction = await _context.Predictions.AsNoTracking().FirstAsync(p => p.PredictionId == prediction.PredictionId);
            var updatedUser = await _context.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);

            Assert.Multiple(() =>
            {
                Assert.That(updatedPrediction.PointsAwarded, Is.EqualTo(50));
                Assert.That(updatedPrediction.AccurateResult, Is.True);
                Assert.That(updatedUser.TotalPoints, Is.EqualTo(50));
                Assert.That(updatedUser.AccurateMatchResults, Is.EqualTo(1));
            });
        }
    }
}
