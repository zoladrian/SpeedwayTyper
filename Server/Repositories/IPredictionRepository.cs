using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public interface IPredictionRepository
    {
        Task<IEnumerable<PredictionModel>> GetPredictionsByUserAsync(string userId);
        Task AddPredictionAsync(PredictionModel prediction);
        Task UpdatePredictionAsync(PredictionModel prediction);
    }

}
