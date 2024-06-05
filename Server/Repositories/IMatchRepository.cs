using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public interface IMatchRepository
    {
        Task<IEnumerable<MatchModel>> GetAllMatchesAsync();
        Task<MatchModel> GetMatchByIdAsync(int matchId);
        Task AddMatchAsync(MatchModel match);
        Task UpdateMatchAsync(MatchModel match);
        Task DeleteMatchAsync(int matchId);
    }
}
