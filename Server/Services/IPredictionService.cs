using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Services
{
    public interface IPredictionService
    {
        Task<int> CalculatePointsAsync(PredictionModel prediction);
        Task AddPredictionAsync(PredictionModel prediction);
        Task UpdatePredictionAsync(PredictionModel prediction);
        Task UpdateUserPointsAsync(string userId);
    }
}
