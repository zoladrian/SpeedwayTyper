using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Server.Services;
using SpeedwayTyperApp.Shared.Models;
using System.Security.Claims;

namespace SpeedwayTyperApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PredictionsController : ControllerBase
    {
        private readonly IPredictionRepository _predictionRepository;
        private readonly IPredictionService _predictionService;

        public PredictionsController(IPredictionRepository predictionRepository, IPredictionService predictionService)
        {
            _predictionRepository = predictionRepository;
            _predictionService = predictionService;
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<PredictionModel>>> GetMyPredictions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var predictions = await _predictionRepository.GetPredictionsByUserAsync(userId);
            return Ok(predictions);
        }

        [HttpPost]
        public async Task<ActionResult<PredictionModel>> CreatePrediction([FromBody] PredictionModel prediction)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            prediction.UserId = userId;
            await _predictionService.AddPredictionAsync(prediction);

            return CreatedAtAction(nameof(GetMyPredictions), new { id = prediction.PredictionId }, prediction);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePrediction(int id, [FromBody] PredictionModel prediction)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            if (id != prediction.PredictionId)
            {
                return BadRequest();
            }

            prediction.UserId = userId;
            await _predictionService.UpdatePredictionAsync(prediction);
            return NoContent();
        }
    }
}
