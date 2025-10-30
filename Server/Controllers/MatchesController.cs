using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Server.Services;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IMatchService _matchService;

        public MatchesController(IMatchRepository matchRepository, IMatchService matchService)
        {
            _matchRepository = matchRepository;
            _matchService = matchService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MatchModel>>> GetMatches()
        {
            var matches = await _matchRepository.GetAllMatchesAsync();
            return Ok(matches);
        }

        [HttpGet("{id}")]
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
            await _matchRepository.AddMatchAsync(match);
            var created = await _matchRepository.GetMatchByIdAsync(match.MatchId);
            return CreatedAtAction(nameof(GetMatch), new { id = match.MatchId }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMatch(int id, [FromBody] MatchModel match)
        {
            if (id != match.MatchId)
            {
                return BadRequest();
            }

            try
            {
                await _matchService.UpdateMatchAsync(match);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMatch(int id)
        {
            await _matchRepository.DeleteMatchAsync(id);
            return NoContent();
        }
    }
}
