
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Server.Services;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Tests
{
    public class PredictionServiceTests
    {
        private Mock<IMatchRepository> _mockMatchRepository = null!;
        private Mock<IUserRepository> _mockUserRepository = null!;
        private Mock<IPredictionRepository> _mockPredictionRepository = null!;
        private PredictionService _predictionService = null!;

        [SetUp]
        public void Setup()
        {
            _mockMatchRepository = new Mock<IMatchRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPredictionRepository = new Mock<IPredictionRepository>();
            _predictionService = new PredictionService(_mockMatchRepository.Object, _mockUserRepository.Object, _mockPredictionRepository.Object);
        }

        [Test]
        public async Task CalculatePointsAsync_ShouldReturn50PointsAndSetAccurateResult_WhenPredictionIsExactTypicalDraw()
        {
            var match = new MatchModel
            {
                MatchId = 1,
                HostTeamScore = 45,
                GuestTeamScore = 45,
                IsCompleted = true
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
        public async Task CalculatePointsAsync_ShouldReturn35PointsAndSetAccurateResult_WhenPredictionIsExactTypicalWin()
        {
            var match = new MatchModel
            {
                MatchId = 2,
                HostTeamScore = 47,
                GuestTeamScore = 43,
                IsCompleted = true
            };

            var prediction = new PredictionModel
            {
                MatchId = 2,
                HostTeamPredictedScore = 47,
                GuestTeamPredictedScore = 43
            };

            _mockMatchRepository.Setup(repo => repo.GetMatchByIdAsync(prediction.MatchId))
                .ReturnsAsync(match);

            var points = await _predictionService.CalculatePointsAsync(prediction);

            Assert.AreEqual(35, points);
            Assert.IsTrue(prediction.AccurateResult);
        }

        [Test]
        public async Task CalculatePointsAsync_ShouldReturn0Points_WhenPredictedDrawButActualHasWinner()
        {
            var match = new MatchModel
            {
                MatchId = 3,
                HostTeamScore = 50,
                GuestTeamScore = 40,
                IsCompleted = true
            };

            var prediction = new PredictionModel
            {
                MatchId = 3,
                HostTeamPredictedScore = 45,
                GuestTeamPredictedScore = 45
            };

            _mockMatchRepository.Setup(repo => repo.GetMatchByIdAsync(prediction.MatchId))
                .ReturnsAsync(match);

            var points = await _predictionService.CalculatePointsAsync(prediction);

            Assert.AreEqual(0, points);
            Assert.IsFalse(prediction.AccurateResult);
        }

        [Test]
        public async Task CalculatePointsAsync_ShouldReturn0Points_WhenPredictedWinnerButActualDraw()
        {
            var match = new MatchModel
            {
                MatchId = 4,
                HostTeamScore = 45,
                GuestTeamScore = 45,
                IsCompleted = true
            };

            var prediction = new PredictionModel
            {
                MatchId = 4,
                HostTeamPredictedScore = 48,
                GuestTeamPredictedScore = 42
            };

            _mockMatchRepository.Setup(repo => repo.GetMatchByIdAsync(prediction.MatchId))
                .ReturnsAsync(match);

            var points = await _predictionService.CalculatePointsAsync(prediction);

            Assert.AreEqual(0, points);
            Assert.IsFalse(prediction.AccurateResult);
        }

        [Test]
        public async Task CalculatePointsAsync_ShouldReturn18Points_WhenPredictionCloseAndWinnerCorrect()
        {
            var match = new MatchModel
            {
                MatchId = 5,
                HostTeamScore = 50,
                GuestTeamScore = 40,
                IsCompleted = true
            };

            var prediction = new PredictionModel
            {
                MatchId = 5,
                HostTeamPredictedScore = 48,
                GuestTeamPredictedScore = 42
            };

            _mockMatchRepository.Setup(repo => repo.GetMatchByIdAsync(prediction.MatchId))
                .ReturnsAsync(match);

            var points = await _predictionService.CalculatePointsAsync(prediction);

            Assert.AreEqual(18, points);
            Assert.IsFalse(prediction.AccurateResult);
        }

        [Test]
        public async Task CalculatePointsAsync_ShouldReturn0Points_WhenPredictionCloseButWinnerIncorrect()
        {
            var match = new MatchModel
            {
                MatchId = 6,
                HostTeamScore = 50,
                GuestTeamScore = 40,
                IsCompleted = true
            };

            var prediction = new PredictionModel
            {
                MatchId = 6,
                HostTeamPredictedScore = 42,
                GuestTeamPredictedScore = 48
            };

            _mockMatchRepository.Setup(repo => repo.GetMatchByIdAsync(prediction.MatchId))
                .ReturnsAsync(match);

            var points = await _predictionService.CalculatePointsAsync(prediction);

            Assert.AreEqual(0, points);
            Assert.IsFalse(prediction.AccurateResult);
        }

        [Test]
        public async Task CalculatePointsAsync_ShouldReturn2Points_WhenPredictionFarOffButWinnerCorrect()
        {
            var match = new MatchModel
            {
                MatchId = 7,
                HostTeamScore = 60,
                GuestTeamScore = 30,
                IsCompleted = true
            };

            var prediction = new PredictionModel
            {
                MatchId = 7,
                HostTeamPredictedScore = 48,
                GuestTeamPredictedScore = 42
            };

            _mockMatchRepository.Setup(repo => repo.GetMatchByIdAsync(prediction.MatchId))
                .ReturnsAsync(match);

            var points = await _predictionService.CalculatePointsAsync(prediction);

            Assert.AreEqual(2, points);
            Assert.IsFalse(prediction.AccurateResult);
        }

        [Test]
        public async Task CalculatePointsAsync_ShouldReturn20Points_WhenPredictionTypicalDrawButActualAtypicalDraw()
        {
            var match = new MatchModel
            {
                MatchId = 8,
                HostTeamScore = 0,
                GuestTeamScore = 0,
                IsCompleted = true
            };

            var prediction = new PredictionModel
            {
                MatchId = 8,
                HostTeamPredictedScore = 45,
                GuestTeamPredictedScore = 45
            };

            _mockMatchRepository.Setup(repo => repo.GetMatchByIdAsync(prediction.MatchId))
                .ReturnsAsync(match);

            var points = await _predictionService.CalculatePointsAsync(prediction);

            Assert.AreEqual(20, points);
            Assert.IsFalse(prediction.AccurateResult);
        }

        [Test]
        public async Task AddPredictionAsync_ShouldAddPredictionAndUpdateUserPoints()
        {
            var match = new MatchModel
            {
                MatchId = 1,
                HostTeamScore = 45,
                GuestTeamScore = 45,
                IsCompleted = true
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
