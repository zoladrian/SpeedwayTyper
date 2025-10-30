using Microsoft.EntityFrameworkCore;
using SpeedwayTyperApp.Server.DbContexts;
using SpeedwayTyperApp.Shared.Models;
using System.Linq;

namespace SpeedwayTyperApp.Server.Repositories
{
    public class RoundRepository : IRoundRepository
    {
        private readonly TypingContext _context;

        public RoundRepository(TypingContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RoundModel>> GetRoundsAsync(int? seasonId = null)
        {
            var query = _context.Rounds
                .AsNoTracking()
                .Include(round => round.Season)
                .AsQueryable();

            if (seasonId.HasValue)
            {
                query = query.Where(round => round.SeasonId == seasonId.Value);
            }

            return await query
                .OrderBy(round => round.Order)
                .ThenBy(round => round.RoundId)
                .ToListAsync();
        }

        public async Task<RoundModel?> GetRoundByIdAsync(int roundId)
        {
            return await _context.Rounds
                .AsNoTracking()
                .Include(round => round.Season)
                .FirstOrDefaultAsync(round => round.RoundId == roundId);
        }

        public async Task<RoundModel> AddRoundAsync(RoundModel round)
        {
            _context.Rounds.Add(round);
            await _context.SaveChangesAsync();
            return round;
        }

        public async Task UpdateRoundAsync(RoundModel round)
        {
            _context.Rounds.Update(round);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRoundAsync(int roundId)
        {
            var round = await _context.Rounds.FindAsync(roundId);
            if (round != null)
            {
                _context.Rounds.Remove(round);
                await _context.SaveChangesAsync();
            }
        }
    }
}
