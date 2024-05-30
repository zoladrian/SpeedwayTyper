using SpeedwayTyperApp.Server.Models;

namespace SpeedwayTyperApp.Server.Services
{
    public interface IPredictionService
    {
        Task<int> CalculatePointsAsync(Prediction prediction);
        Task AddPredictionAsync(Prediction prediction);
        Task UpdatePredictionAsync(Prediction prediction);
        Task UpdateUserPointsAsync(string userId);
    }
}
