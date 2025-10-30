using Moq;
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Server.Services;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Tests
{
    public class PredictionServiceTests
    {
        private Mock<IMatchRepository> _mockMatchRepository;
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IPredictionRepository> _mockPredictionRepository;
        private PredictionService _predictionService;

        [SetUp]
        public void Setup()
        {
            _mockMatchRepository = new Mock<IMatchRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPredictionRepository = new Mock<IPredictionRepository>();
            _predictionService = new PredictionService(_mockMatchRepository.Object, _mockUserRepository.Object, _mockPredictionRepository.Object);
        }

        [Test]
        public async Task CalculatePointsAsync_ShouldReturn50PointsAndSetAccurateResult_WhenPredictionIsExactDraw()
        {
            var match = new MatchModel
            {
                MatchId = 1,
                HostScore = 45,
                GuestScore = 45,
                Status = MatchStatus.Completed
            };

            var prediction = new PredictionModel
            {
                MatchId = 1,
                HostTeamPredictedScore = 45,
                GuestTeamPredictedScore = 45
            };

            _mockMatchRepository.Setup(repo => repo.GetMatchByIdAsync(prediction.MatchId))
                .ReturnsAsync(match);

            var points = await _predictionService.CalculatePointsAsync(prediction);

            Assert.AreEqual(50, points);
            Assert.IsTrue(prediction.AccurateResult);
        }

        [Test]
        public async Task AddPredictionAsync_ShouldAddPredictionAndUpdateUserPoints()
        {
            var match = new MatchModel
            {
                MatchId = 1,
                HostScore = 45,
                GuestScore = 45,
                Status = MatchStatus.Completed
            };

            var prediction = new PredictionModel
            {
                MatchId = 1,
                UserId = "user1",
                HostTeamPredictedScore = 45,
                GuestTeamPredictedScore = 45
            };

            var user = new UserModel
            {
                Id = "user1",
                TotalPoints = 0,
                AccurateMatchResults = 0
            };

            _mockMatchRepository.Setup(repo => repo.GetMatchByIdAsync(prediction.MatchId))
                .ReturnsAsync(match);
            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(prediction.UserId))
                .ReturnsAsync(user);
            _mockPredictionRepository.Setup(repo => repo.AddPredictionAsync(prediction))
                .Returns(Task.CompletedTask);
            _mockPredictionRepository.Setup(repo => repo.GetPredictionsByUserAsync(prediction.UserId))
                .ReturnsAsync(new List<PredictionModel> { prediction });

            await _predictionService.AddPredictionAsync(prediction);

            _mockPredictionRepository.Verify(repo => repo.AddPredictionAsync(prediction), Times.Once);
            _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.Is<UserModel>(u => u.TotalPoints == 50 && u.AccurateMatchResults == 1)), Times.Once);
        }
    }
}
