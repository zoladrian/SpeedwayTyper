using Microsoft.AspNetCore.Mvc;
using SpeedwayTyperApp.Shared.Models;
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Server.Services;

namespace SpeedwayTyperApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPredictionService _predictionService;

        public UsersController(IUserRepository userRepository, IPredictionService predictionService)
        {
            _userRepository = userRepository;
            _predictionService = predictionService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet("{id}/points")]
        public async Task<IActionResult> GetUserPoints(string id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user.TotalPoints);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserModel user)
        {
            if (!id.Equals(user.Id))
            {
                return BadRequest();
            }

            await _userRepository.UpdateUserAsync(user);
            return NoContent();
        }
    }
}
