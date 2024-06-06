using Moq;
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Server.Services;
using SpeedwayTyperApp.Shared.Models;
using System.Diagnostics;

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
        //Nazwa testu to nazwa testowanej metody, co testujemy i wynik
        public async Task CalculatePointsAsync_ShouldReturn50PointsAndSetAccurateResult_WhenPredictionIsExactlyDraw()
        {
            // Arrange
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

            //za pomoc¹ .Setup ustawiamy ¿eby GetMatchByIdAsync z danym argumentem zwraca³o
            //ten konkretny obiekt match
            _mockMatchRepository.Setup(repo => repo.GetMatchByIdAsync(prediction.MatchId))
                .ReturnsAsync(match);

            // Act
            var points = await _predictionService.CalculatePointsAsync(prediction);

            // Assert
            Assert.AreEqual(50, points);
            Assert.IsTrue(prediction.AccurateResult);
        }

        [Test]
        //1. Gracz wytypowa³ dok³adny wynik, ale nie remis i jest to wynik "typowy" w ¿u¿lu (np. 43:47, czyli suma wyniku to 90) (35pkt i trafiony wynik na true).

        public async Task CalculatePointsAsync_ShouldReturn35PointsAndSetAccurateResult_WhenPredictionIsExactlySameAsScorre()
        {
            var match = new MatchModel
            {
                MatchId = 1,
                HostTeamScore = 50,
                GuestTeamScore = 40,
                IsCompleted = true
            };

            var prediction = new PredictionModel
            {
                MatchId = 1,
                HostTeamPredictedScore = 50,
                GuestTeamPredictedScore = 40
            };

            _mockMatchRepository.Setup(repo => repo.GetMatchByIdAsync(prediction.MatchId))
                .ReturnsAsync(match);

            // Act
            var points = await _predictionService.CalculatePointsAsync(prediction);

            // Assert
            Assert.AreEqual(35, points);
            Assert.IsTrue(prediction.AccurateResult);
        }

        [Test]
        //2. Gracz wytypowa³ remis a wygra³a któraœ z dru¿yn(nie powinien dostaæ punktów)

        public async Task CalculatePointsAsync_ShouldReturn0Points_WhenPredictionIsDrawButTeamWin()
        {
            var match = new MatchModel
            {
                MatchId = 1,
                HostTeamScore = 50,
                GuestTeamScore = 40,
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

            // Act
            var points = await _predictionService.CalculatePointsAsync(prediction);

            // Assert
            Assert.AreEqual(0, points);
        }

        [Test]
        //3. Gracz wytypowa³ wygran¹ jednej z dru¿yn ale pad³ remis (nie powinien dostaæ punktów)


        public async Task CalculatePointsAsync_ShouldReturn0Points_WhenPredictionIsTeamWinButIsDraw()
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
                HostTeamPredictedScore = 50,
                GuestTeamPredictedScore = 40
            };

            _mockMatchRepository.Setup(repo => repo.GetMatchByIdAsync(prediction.MatchId))
                .ReturnsAsync(match);

            // Act
            var points = await _predictionService.CalculatePointsAsync(prediction);

            // Assert
            Assert.AreEqual(0, points);
        }

        [Test]
        //4. Gracz pomyli³ siê o mniej ni¿ 20 pkt (np. 36:54) ale postawi³ na zwyciêzce (pkt wed³ug tabeli)
        public async Task CalculatePointsAsync_ShouldReturn0Points_WhenPredictionIsPredictedTeamWin_AndPointsErrorIsLessThan20()
        {
            var match = new MatchModel
            {
                MatchId = 1,
                HostTeamScore = 50,
                GuestTeamScore = 40,
                IsCompleted = true
            };

            var prediction = new PredictionModel
            {
                MatchId = 1,
                HostTeamPredictedScore = 54,
                GuestTeamPredictedScore = 36
            };

            _mockMatchRepository.Setup(repo => repo.GetMatchByIdAsync(prediction.MatchId))
                .ReturnsAsync(match);

            // Act
            var points = await _predictionService.CalculatePointsAsync(prediction);

            // Assert
            Assert.AreEqual(14, points);
        }

        [Test]
        //5. Gracz pomyli³ siê o mniej ni¿ 20 pkt i postawi³ na przegranego (nie powinien dostaæ punktów)
        public async Task CalculatePointsAsync_ShouldReturn0Points_WhenPredictionIsOneTeamWinButSecondTeamWin_AndPointsErrorIsLessThan20()
        {
            var match = new MatchModel
            {
                MatchId = 1,
                HostTeamScore = 44,
                GuestTeamScore = 46,
                IsCompleted = true
            };

            var prediction = new PredictionModel
            {
                MatchId = 1,
                HostTeamPredictedScore = 50,
                GuestTeamPredictedScore = 40
            };

            _mockMatchRepository.Setup(repo => repo.GetMatchByIdAsync(prediction.MatchId))
                .ReturnsAsync(match);

            // Act
            var points = await _predictionService.CalculatePointsAsync(prediction);

            // Assert
            Assert.AreEqual(0, points);
        }

        [Test]
        // 6. Gracz pomyli³ siê o wiêcej ni¿ 20pkt i postawi³ na wygran¹ dru¿yne (2pkt)
        public async Task CalculatePointsAsync_ShouldReturn0Points_WhenPredictionIsPredictedTeamWin_AndPointsErrorIsMoreThan20()
        {
            var match = new MatchModel
            {
                MatchId = 1,
                HostTeamScore = 46,
                GuestTeamScore = 44,
                IsCompleted = true
            };

            var prediction = new PredictionModel
            {
                MatchId = 1,
                HostTeamPredictedScore = 64,
                GuestTeamPredictedScore = 26
            };

            _mockMatchRepository.Setup(repo => repo.GetMatchByIdAsync(prediction.MatchId))
                .ReturnsAsync(match);

            // Act
            var points = await _predictionService.CalculatePointsAsync(prediction);

            // Assert
            Assert.AreEqual(2, points);
        }
        [Test]
        //7. Gracz postawi³ 45:45 ale pad³ wynik 0:0 (jak ostatnio podwójny walkower z grudzi¹dzem xD) (powinien dostaæ 20pkt)
        public async Task CalculatePointsAsync_ShouldReturn20Points_WhenPredictionIsOK_ButMatchEndWalkover()
        {
            var match = new MatchModel
            {
                MatchId = 1,
                HostTeamScore = 0,
                GuestTeamScore = 0,
                IsCompleted = true
            };

            var prediction = new PredictionModel
            {
                MatchId = 1,
                HostTeamPredictedScore = 64,
                GuestTeamPredictedScore = 26
            };

            _mockMatchRepository.Setup(repo => repo.GetMatchByIdAsync(prediction.MatchId))
                .ReturnsAsync(match);

            // Act
            var points = await _predictionService.CalculatePointsAsync(prediction);

            // Assert
            Assert.AreEqual(20, points);
        }

        [Test]
        //tutaj przyk³ad testu dla innej metody która updatuje usera, doda³em j¹ ¿eby by³o widaæ bardziej rozszerzone dzia³anie
        //mocków. Poza "udawaniem" obiektów implementuj¹cych dany interface, sprawdzaj¹ czy metody tych udawanych
        //obiektów s¹ wywo³ywane z konkretnymi danymi.
        public async Task AddPredictionAsync_ShouldAddPredictionAndUpdateUserPoints()
        {
            // Arrange
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

            // Act
            await _predictionService.AddPredictionAsync(prediction);

            // Assert
            //.Verify na mocku oznacza ¿e sprawdzamy czy to zadzia³a³o ta metoda z tym argumentem zosta³a jednokrotnie wywo³ana
            _mockPredictionRepository.Verify(repo => repo.AddPredictionAsync(prediction), Times.Once);
            //tutaj bardziej rozszerzone u¿ycie, sprawdzamy czy przekazany by³ obiekt 
            //typu UserModel z konkretn¹ wartoœci¹ dla TotalPoints i AccurateMatchResult
            _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.Is<UserModel>(u => u.TotalPoints == 50 && u.AccurateMatchResults == 1)), Times.Once);
        }
    }
}