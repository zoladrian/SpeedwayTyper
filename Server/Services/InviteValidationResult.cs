using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Services
{
    public record InviteValidationResult(bool Success, string? ErrorMessage, InviteCodeModel? Invite = null);
}
