using Microsoft.EntityFrameworkCore;
using SpeedwayTyperApp.Server.DbContexts;
using SpeedwayTyperApp.Shared.Models;
using System.Linq;

namespace SpeedwayTyperApp.Server.Repositories
{
    public class PredictionRepository : IPredictionRepository
    {
        private readonly TypingContext _context;

        public PredictionRepository(TypingContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PredictionModel>> GetPredictionsByUserAsync(string userId)
        {
            return await _context.Predictions
                .Where(p => p.UserId.Equals(userId))
                .ToListAsync();
        }

        public async Task AddPredictionAsync(PredictionModel prediction)
        {
            _context.Predictions.Add(prediction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePredictionAsync(PredictionModel prediction)
        {
            _context.Predictions.Update(prediction);
            await _context.SaveChangesAsync();
        }
    }
}
