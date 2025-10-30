using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public interface IRoundRepository
    {
        Task<IEnumerable<RoundModel>> GetRoundsAsync(int? seasonId = null);
        Task<RoundModel?> GetRoundByIdAsync(int roundId);
        Task<RoundModel> AddRoundAsync(RoundModel round);
        Task UpdateRoundAsync(RoundModel round);
        Task DeleteRoundAsync(int roundId);
    }
}
