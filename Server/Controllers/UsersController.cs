
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Server.Services;
using SpeedwayTyperApp.Shared.Models;
using System;
using System.Security.Claims;

namespace SpeedwayTyperApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPredictionRepository _predictionRepository;
        private readonly IPredictionService _predictionService;

        public UsersController(
            IUserRepository userRepository,
            IPredictionRepository predictionRepository,
            IPredictionService predictionService)
        {
            _userRepository = userRepository;
            _predictionRepository = predictionRepository;
            _predictionService = predictionService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            if (!CanAccessUser(id))
            {
                return Forbid();
            }

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
            if (!CanAccessUser(id))
            {
                return Forbid();
            }

            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user.TotalPoints);
        }

        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetUserHistory(string id)
        {
            if (!CanAccessUser(id))
            {
                return Forbid();
            }

            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var predictions = await _predictionRepository.GetPredictionsByUserAsync(id);
            return Ok(predictions);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserModel user)
        {
            if (user == null || !id.Equals(user.Id))
            {
                return BadRequest();
            }

            if (!CanAccessUser(id))
            {
                return Forbid();
            }

            await _userRepository.UpdateUserAsync(user);
            await _predictionService.UpdateUserPointsAsync(id);
            return NoContent();
        }

        private bool CanAccessUser(string userId)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrWhiteSpace(userId) && string.Equals(currentUserId, userId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
