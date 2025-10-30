using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpeedwayTyperApp.Server.Controllers;
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Server.Services;
using SpeedwayTyperApp.Shared.Models;
using System.Linq;
using System.Security.Claims;

namespace SpeedwayTyperApp.Server.Tests
{
    public class PicksControllerTests
    {
        private readonly Mock<IPredictionRepository> _predictionRepository = new();
        private readonly Mock<IPredictionService> _predictionService = new();
        private readonly Mock<IMatchRepository> _matchRepository = new();

        private PicksController CreateController(string userId, params string[] roles)
        {
            var controller = new PicksController(_predictionRepository.Object, _predictionService.Object, _matchRepository.Object);
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            var identity = new ClaimsIdentity(claims, "TestAuth");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };
            return controller;
        }

        [Test]
        public async Task CreatePrediction_ShouldReturnBadRequest_WhenMatchIsLocked()
        {
            var userId = "user-1";
            var controller = CreateController(userId);
            var match = new MatchModel { MatchId = 1, Date = DateTime.UtcNow.AddMinutes(-10), IsCompleted = true };
            _matchRepository.Setup(m => m.GetMatchByIdAsync(match.MatchId)).ReturnsAsync(match);

            var prediction = new PredictionModel
            {
                MatchId = match.MatchId,
                UserId = userId,
                HostTeamPredictedScore = 45,
                GuestTeamPredictedScore = 45
            };

            var result = await controller.CreatePrediction(prediction);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest!.Value, Is.EqualTo("Predictions are locked for this match."));
            _predictionService.Verify(s => s.AddPredictionAsync(It.IsAny<PredictionModel>()), Times.Never);
        }

        [Test]
        public async Task UpdatePrediction_ShouldReturnBadRequest_WhenMatchIsLocked()
        {
            var userId = "user-2";
            var controller = CreateController(userId);
            var existing = new PredictionModel
            {
                PredictionId = 10,
                MatchId = 2,
                UserId = userId,
                HostTeamPredictedScore = 46,
                GuestTeamPredictedScore = 44
            };

            _predictionRepository.Setup(r => r.GetPredictionByIdAsync(existing.PredictionId)).ReturnsAsync(existing);
            _matchRepository.Setup(m => m.GetMatchByIdAsync(existing.MatchId))
                            .ReturnsAsync(new MatchModel { MatchId = existing.MatchId, Date = DateTime.UtcNow.AddMinutes(-5), IsCompleted = false });

            var result = await controller.UpdatePrediction(existing.PredictionId, existing);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            _predictionService.Verify(s => s.UpdatePredictionAsync(It.IsAny<PredictionModel>()), Times.Never);
        }

        [Test]
        public async Task CreatePrediction_ShouldReturnCreated_WhenMatchIsOpen()
        {
            var userId = "user-3";
            var controller = CreateController(userId);
            var match = new MatchModel { MatchId = 3, Date = DateTime.UtcNow.AddHours(2), IsCompleted = false };
            _matchRepository.Setup(m => m.GetMatchByIdAsync(match.MatchId)).ReturnsAsync(match);
            _predictionRepository.Setup(r => r.GetPredictionByUserAndMatchAsync(userId, match.MatchId)).ReturnsAsync((PredictionModel?)null);
            _predictionService.Setup(s => s.AddPredictionAsync(It.IsAny<PredictionModel>()))
                              .Callback<PredictionModel>(p => p.PredictionId = 99)
                              .Returns(Task.CompletedTask);

            var prediction = new PredictionModel
            {
                MatchId = match.MatchId,
                UserId = userId,
                HostTeamPredictedScore = 50,
                GuestTeamPredictedScore = 40
            };

            var result = await controller.CreatePrediction(prediction);

            var createdResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(nameof(PicksController.GetPrediction), createdResult!.ActionName);
            Assert.AreEqual(99, ((PredictionModel)createdResult.Value!).PredictionId);
            _predictionService.Verify(s => s.AddPredictionAsync(It.Is<PredictionModel>(p => p.UserId == userId)), Times.Once);
        }
    }
}
