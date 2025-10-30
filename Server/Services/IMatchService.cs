using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Services
{
    public interface IMatchService
    {
        Task UpdateMatchAsync(MatchModel match);
    }
}
