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

            //za pomoc� .Setup ustawiamy �eby GetMatchByIdAsync z danym argumentem zwraca�o
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
        //1. Gracz wytypowa� dok�adny wynik, ale nie remis i jest to wynik "typowy" w �u�lu (np. 43:47, czyli suma wyniku to 90) (35pkt i trafiony wynik na true).

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
        //2. Gracz wytypowa� remis a wygra�a kt�ra� z dru�yn(nie powinien dosta� punkt�w)

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
        //3. Gracz wytypowa� wygran� jednej z dru�yn ale pad� remis (nie powinien dosta� punkt�w)


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
        //4. Gracz pomyli� si� o mniej ni� 20 pkt (np. 36:54) ale postawi� na zwyci�zce (pkt wed�ug tabeli)
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
        //5. Gracz pomyli� si� o mniej ni� 20 pkt i postawi� na przegranego (nie powinien dosta� punkt�w)
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
        // 6. Gracz pomyli� si� o wi�cej ni� 20pkt i postawi� na wygran� dru�yne (2pkt)
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
        //7. Gracz postawi� 45:45 ale pad� wynik 0:0 (jak ostatnio podw�jny walkower z grudzi�dzem xD) (powinien dosta� 20pkt)
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
        //tutaj przyk�ad testu dla innej metody kt�ra updatuje usera, doda�em j� �eby by�o wida� bardziej rozszerzone dzia�anie
        //mock�w. Poza "udawaniem" obiekt�w implementuj�cych dany interface, sprawdzaj� czy metody tych udawanych
        //obiekt�w s� wywo�ywane z konkretnymi danymi.
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
            //.Verify na mocku oznacza �e sprawdzamy czy to zadzia�a�o ta metoda z tym argumentem zosta�a jednokrotnie wywo�ana
            _mockPredictionRepository.Verify(repo => repo.AddPredictionAsync(prediction), Times.Once);
            //tutaj bardziej rozszerzone u�ycie, sprawdzamy czy przekazany by� obiekt 
            //typu UserModel z konkretn� warto�ci� dla TotalPoints i AccurateMatchResult
            _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.Is<UserModel>(u => u.TotalPoints == 50 && u.AccurateMatchResults == 1)), Times.Once);
        }
    }
}