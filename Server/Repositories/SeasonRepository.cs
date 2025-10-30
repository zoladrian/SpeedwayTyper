using Microsoft.EntityFrameworkCore;
using SpeedwayTyperApp.Server.DbContexts;
using SpeedwayTyperApp.Shared.Models;
using System.Linq;

namespace SpeedwayTyperApp.Server.Repositories
{
    public class SeasonRepository : ISeasonRepository
    {
        private readonly TypingContext _context;

        public SeasonRepository(TypingContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SeasonModel>> GetSeasonsAsync()
        {
            return await _context.Seasons
                .AsNoTracking()
                .OrderByDescending(season => season.IsActive)
                .ThenByDescending(season => season.StartDateUtc)
                .ToListAsync();
        }

        public async Task<SeasonModel?> GetSeasonByIdAsync(int seasonId)
        {
            return await _context.Seasons.AsNoTracking().FirstOrDefaultAsync(season => season.SeasonId == seasonId);
        }

        public async Task<SeasonModel> AddSeasonAsync(SeasonModel season)
        {
            _context.Seasons.Add(season);
            await _context.SaveChangesAsync();
            return season;
        }

        public async Task UpdateSeasonAsync(SeasonModel season)
        {
            _context.Seasons.Update(season);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSeasonAsync(int seasonId)
        {
            var season = await _context.Seasons.FindAsync(seasonId);
            if (season != null)
            {
                _context.Seasons.Remove(season);
                await _context.SaveChangesAsync();
            }
        }
    }
}
