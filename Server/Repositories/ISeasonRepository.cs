using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public interface ISeasonRepository
    {
        Task<IEnumerable<SeasonModel>> GetSeasonsAsync();
        Task<SeasonModel?> GetSeasonByIdAsync(int seasonId);
        Task<SeasonModel> AddSeasonAsync(SeasonModel season);
        Task UpdateSeasonAsync(SeasonModel season);
        Task DeleteSeasonAsync(int seasonId);
    }
}
