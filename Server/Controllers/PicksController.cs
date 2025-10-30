using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Server.Services;
using SpeedwayTyperApp.Shared.Models;
using System;
using System.Security.Claims;

namespace SpeedwayTyperApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PicksController : ControllerBase
    {
        private readonly IPredictionRepository _predictionRepository;
        private readonly IPredictionService _predictionService;
        private readonly IMatchRepository _matchRepository;

        public PicksController(
            IPredictionRepository predictionRepository,
            IPredictionService predictionService,
            IMatchRepository matchRepository)
        {
            _predictionRepository = predictionRepository;
            _predictionService = predictionService;
            _matchRepository = matchRepository;
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyPredictions()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var predictions = await _predictionRepository.GetPredictionsByUserAsync(currentUserId);
            return Ok(predictions);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPredictionsForUser(string userId)
        {
            if (!CanAccessUser(userId))
            {
                return Forbid();
            }

            var predictions = await _predictionRepository.GetPredictionsByUserAsync(userId);
            return Ok(predictions);
        }

        [HttpGet("{predictionId:int}")]
        public async Task<IActionResult> GetPrediction(int predictionId)
        {
            var prediction = await _predictionRepository.GetPredictionByIdAsync(predictionId);
            if (prediction == null)
            {
                return NotFound();
            }

            if (!CanAccessUser(prediction.UserId))
            {
                return Forbid();
            }

            return Ok(prediction);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePrediction([FromBody] PredictionModel prediction)
        {
            if (prediction == null)
            {
                return BadRequest("Prediction payload is required.");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            if (!User.IsInRole("Admin") || string.IsNullOrEmpty(prediction.UserId))
            {
                prediction.UserId = currentUserId;
            }

            if (!CanAccessUser(prediction.UserId))
            {
                return Forbid();
            }

            var match = await _matchRepository.GetMatchByIdAsync(prediction.MatchId);
            if (match == null)
            {
                return NotFound("Match not found.");
            }

            if (IsMatchLocked(match))
            {
                return BadRequest("Predictions are locked for this match.");
            }

            var existing = await _predictionRepository.GetPredictionByUserAndMatchAsync(prediction.UserId, prediction.MatchId);
            if (existing != null)
            {
                return Conflict("Prediction already submitted for this match.");
            }

            prediction.CreatedAt = DateTime.UtcNow;

            await _predictionService.AddPredictionAsync(prediction);

            return CreatedAtAction(nameof(GetPrediction), new { predictionId = prediction.PredictionId }, prediction);
        }

        [HttpPut("{predictionId:int}")]
        public async Task<IActionResult> UpdatePrediction(int predictionId, [FromBody] PredictionModel prediction)
        {
            if (prediction == null || predictionId != prediction.PredictionId)
            {
                return BadRequest("Prediction identifier mismatch.");
            }

            var existing = await _predictionRepository.GetPredictionByIdAsync(predictionId);
            if (existing == null)
            {
                return NotFound();
            }

            if (!CanAccessUser(existing.UserId))
            {
                return Forbid();
            }

            if (prediction.MatchId != existing.MatchId)
            {
                return BadRequest("Match selection cannot be changed for an existing prediction.");
            }

            var match = await _matchRepository.GetMatchByIdAsync(existing.MatchId);
            if (match == null)
            {
                return NotFound("Match not found.");
            }

            if (IsMatchLocked(match))
            {
                return BadRequest("Predictions are locked for this match.");
            }

            existing.HostTeamPredictedScore = prediction.HostTeamPredictedScore;
            existing.GuestTeamPredictedScore = prediction.GuestTeamPredictedScore;

            await _predictionService.UpdatePredictionAsync(existing);
            return NoContent();
        }

        private static bool IsMatchLocked(MatchModel match)
        {
            return match.IsCompleted || match.Date <= DateTime.UtcNow;
        }

        private bool CanAccessUser(string userId)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrEmpty(userId) && string.Equals(currentUserId, userId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
