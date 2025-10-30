using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeasonsController : ControllerBase
    {
        private readonly ISeasonRepository _seasonRepository;

        public SeasonsController(ISeasonRepository seasonRepository)
        {
            _seasonRepository = seasonRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SeasonModel>>> GetSeasons()
        {
            var seasons = await _seasonRepository.GetSeasonsAsync();
            return Ok(seasons);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<SeasonModel>> GetSeason(int id)
        {
            var season = await _seasonRepository.GetSeasonByIdAsync(id);
            if (season == null)
            {
                return NotFound();
            }

            return Ok(season);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SeasonModel>> CreateSeason([FromBody] SeasonModel season)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var createdSeason = await _seasonRepository.AddSeasonAsync(season);
            return CreatedAtAction(nameof(GetSeason), new { id = createdSeason.SeasonId }, createdSeason);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSeason(int id, [FromBody] SeasonModel season)
        {
            if (id != season.SeasonId)
            {
                return BadRequest("The season identifier cannot be modified.");
            }

            var existing = await _seasonRepository.GetSeasonByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            await _seasonRepository.UpdateSeasonAsync(season);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSeason(int id)
        {
            var existing = await _seasonRepository.GetSeasonByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            await _seasonRepository.DeleteSeasonAsync(id);
            return NoContent();
        }
    }
}
