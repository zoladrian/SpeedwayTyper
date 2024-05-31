using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public interface IPredictionRepository
    {
        Task<IEnumerable<Prediction>> GetPredictionsByUserAsync(string userId);
        Task AddPredictionAsync(Prediction prediction);
        Task UpdatePredictionAsync(Prediction prediction);
    }

}
