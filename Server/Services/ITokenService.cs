using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}