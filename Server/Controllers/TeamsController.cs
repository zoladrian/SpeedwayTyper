using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamRepository _teamRepository;

        public TeamsController(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamModel>>> GetTeams()
        {
            var teams = await _teamRepository.GetTeamsAsync();
            return Ok(teams);
        }
    }
}
