using System.Collections.Generic;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Services
{
    public interface ITokenService
    {
        string GenerateToken(UserModel user, IEnumerable<string> roles);
    }
}