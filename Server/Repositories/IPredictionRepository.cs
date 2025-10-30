using System.Collections.Generic;
using System.Threading.Tasks;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public interface IPredictionRepository
    {
        Task<IEnumerable<PredictionModel>> GetPredictionsByUserAsync(string userId);
        Task<PredictionModel?> GetPredictionByIdAsync(int predictionId);
        Task<PredictionModel?> GetPredictionByUserAndMatchAsync(string userId, int matchId);
        Task<IEnumerable<PredictionModel>> GetPredictionsForMatchAsync(int matchId);
        Task AddPredictionAsync(PredictionModel prediction);
        Task UpdatePredictionAsync(PredictionModel prediction);
    }
}
