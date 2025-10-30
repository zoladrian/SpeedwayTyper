using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoundsController : ControllerBase
    {
        private readonly IRoundRepository _roundRepository;

        public RoundsController(IRoundRepository roundRepository)
        {
            _roundRepository = roundRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RoundModel>>> GetRounds([FromQuery] int? seasonId)
        {
            var rounds = await _roundRepository.GetRoundsAsync(seasonId);
            return Ok(rounds);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<RoundModel>> GetRound(int id)
        {
            var round = await _roundRepository.GetRoundByIdAsync(id);
            if (round == null)
            {
                return NotFound();
            }

            return Ok(round);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RoundModel>> CreateRound([FromBody] RoundModel round)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var createdRound = await _roundRepository.AddRoundAsync(round);
            return CreatedAtAction(nameof(GetRound), new { id = createdRound.RoundId }, createdRound);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRound(int id, [FromBody] RoundModel round)
        {
            if (id != round.RoundId)
            {
                return BadRequest("The round identifier cannot be modified.");
            }

            var existing = await _roundRepository.GetRoundByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            await _roundRepository.UpdateRoundAsync(round);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRound(int id)
        {
            var existing = await _roundRepository.GetRoundByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            await _roundRepository.DeleteRoundAsync(id);
            return NoContent();
        }
    }
}
