using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpeedwayTyperApp.Server.Services;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IInviteService _inviteService;

        public AuthController(UserManager<UserModel> userManager, ITokenService tokenService, IInviteService inviteService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _inviteService = inviteService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return Unauthorized();
            }

            if (user.IsPendingApproval)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Konto oczekuje na zatwierdzenie przez administratora.");
            }

            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = _tokenService.GenerateToken(user);
                return Ok(new { Token = token });
            }

            return Unauthorized();
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                return BadRequest("New password and confirmation password do not match.");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("register-by-invite")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterByInvite([FromBody] RegisterByInviteModel model)
        {
            var validation = await _inviteService.ValidateInviteAsync(model.InviteCode);
            if (!validation.Success)
            {
                return BadRequest(validation.ErrorMessage);
            }

            var user = new UserModel
            {
                UserName = model.Username,
                Email = model.Email,
                IsPendingApproval = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await _userManager.AddToRoleAsync(user, "Player");

            var consumeResult = await _inviteService.TryConsumeInviteAsync(model.InviteCode);
            if (!consumeResult.Success)
            {
                await _userManager.DeleteAsync(user);
                return BadRequest(consumeResult.ErrorMessage);
            }

            return Ok();
        }
    }
}
