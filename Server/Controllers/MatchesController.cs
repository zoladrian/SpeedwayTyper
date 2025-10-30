using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchRepository _matchRepository;

        public MatchesController(IMatchRepository matchRepository)
        {
            _matchRepository = matchRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MatchModel>>> GetMatches([FromQuery] int? seasonId, [FromQuery] int? roundId)
        {
            var matches = await _matchRepository.GetMatchesAsync(seasonId, roundId);
            return Ok(matches);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<MatchModel>> GetMatch(int id)
        {
            var match = await _matchRepository.GetMatchByIdAsync(id);
            if (match == null)
            {
                return NotFound();
            }

            return Ok(match);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MatchModel>> CreateMatch([FromBody] MatchModel match)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var createdMatch = await _matchRepository.AddMatchAsync(match);
            return CreatedAtAction(nameof(GetMatch), new { id = createdMatch.MatchId }, createdMatch);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMatch(int id, [FromBody] MatchModel match)
        {
            if (id != match.MatchId)
            {
                return BadRequest("The match identifier cannot be modified.");
            }

            var existing = await _matchRepository.GetMatchByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            await _matchRepository.UpdateMatchAsync(match);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMatch(int id)
        {
            var existing = await _matchRepository.GetMatchByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            await _matchRepository.DeleteMatchAsync(id);
            return NoContent();
        }
    }
}
