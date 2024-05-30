using Microsoft.EntityFrameworkCore;
using SpeedwayTyperApp.Server.DbContexts;
using SpeedwayTyperApp.Server.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public class PredictionRepository : IPredictionRepository
    {
        private readonly TypingContext _context;

        public PredictionRepository(TypingContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Prediction>> GetPredictionsByUserAsync(string userId)
        {
            return await _context.Predictions.Where(p => p.UserId.Equals(userId)).ToListAsync();
        }

        public async Task AddPredictionAsync(Prediction prediction)
        {
            _context.Predictions.Add(prediction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePredictionAsync(Prediction prediction)
        {
            _context.Predictions.Update(prediction);
            await _context.SaveChangesAsync();
        }
    }

}
