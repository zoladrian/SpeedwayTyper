using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Services
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(UserModel user);
    }
}