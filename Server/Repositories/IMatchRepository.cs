using SpeedwayTyperApp.Server.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public interface IMatchRepository
    {
        Task<IEnumerable<Match>> GetAllMatchesAsync();
        Task<Match> GetMatchByIdAsync(int matchId);
        Task AddMatchAsync(Match match);
        Task UpdateMatchAsync(Match match);
        Task DeleteMatchAsync(int matchId);
    }
}
