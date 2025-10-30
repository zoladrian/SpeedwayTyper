using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public interface IMatchRepository
    {
        Task<IEnumerable<MatchModel>> GetMatchesAsync(int? seasonId = null, int? roundId = null);
        Task<MatchModel?> GetMatchByIdAsync(int matchId);
        Task<MatchModel> AddMatchAsync(MatchModel match);
        Task UpdateMatchAsync(MatchModel match);
        Task DeleteMatchAsync(int matchId);
    }
}
