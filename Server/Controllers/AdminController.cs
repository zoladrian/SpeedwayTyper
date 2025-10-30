using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Server.Services;
using SpeedwayTyperApp.Shared.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace SpeedwayTyperApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly IInviteService _inviteService;
        private readonly IUserRepository _userRepository;

        public AdminController(UserManager<UserModel> userManager, IInviteService inviteService, IUserRepository userRepository)
        {
            _userManager = userManager;
            _inviteService = inviteService;
            _userRepository = userRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new UserModel { UserName = model.Username, Email = model.Email, IsPendingApproval = false };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        [HttpGet("invites")]
        public async Task<IActionResult> GetInvites()
        {
            var invites = await _inviteService.GetInvitesAsync();
            return Ok(invites);
        }

        [HttpPost("invites")]
        public async Task<IActionResult> CreateInvite([FromBody] InviteCreateModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name ?? userId ?? string.Empty;
            if (userId == null)
            {
                return Unauthorized();
            }
            try
            {
                var invite = await _inviteService.CreateInviteAsync(model, userId, userName);
                return Ok(invite);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("users/pending")]
        public async Task<IActionResult> GetPendingUsers()
        {
            var pendingUsers = await _userRepository.GetPendingUsersAsync();
            var result = new List<AdminUserModel>();

            foreach (var user in pendingUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new AdminUserModel
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    IsPendingApproval = user.IsPendingApproval,
                    Roles = roles
                });
            }

            return Ok(result);
        }

        [HttpPost("users/{id}/approve")]
        public async Task<IActionResult> ApproveUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsPendingApproval = false;
            await _userManager.UpdateAsync(user);
            return NoContent();
        }

        [HttpPost("users/{id}/promote")]
        public async Task<IActionResult> PromoteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            user.IsPendingApproval = false;
            await _userManager.UpdateAsync(user);

            return NoContent();
        }
    }
}
