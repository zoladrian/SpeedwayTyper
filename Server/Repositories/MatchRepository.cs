using Microsoft.EntityFrameworkCore;
using SpeedwayTyperApp.Server.DbContexts;
using SpeedwayTyperApp.Shared.Models;
using System.Linq;

namespace SpeedwayTyperApp.Server.Repositories
{
    public class MatchRepository : IMatchRepository
    {
        private readonly TypingContext _context;

        public MatchRepository(TypingContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MatchModel>> GetMatchesAsync(int? seasonId = null, int? roundId = null)
        {
            var query = _context.Matches
                .AsNoTracking()
                .Include(match => match.HostTeam)
                .Include(match => match.GuestTeam)
                .Include(match => match.Season)
                .Include(match => match.Round)
                .AsQueryable();

            if (seasonId.HasValue)
            {
                query = query.Where(match => match.SeasonId == seasonId.Value);
            }

            if (roundId.HasValue)
            {
                query = query.Where(match => match.RoundId == roundId.Value);
            }

            return await query
                .OrderBy(match => match.StartTimeUtc)
                .ToListAsync();
        }

        public async Task<MatchModel?> GetMatchByIdAsync(int matchId)
        {
            return await _context.Matches
                .AsNoTracking()
                .Include(match => match.HostTeam)
                .Include(match => match.GuestTeam)
                .Include(match => match.Season)
                .Include(match => match.Round)
                .FirstOrDefaultAsync(match => match.MatchId == matchId);
        }

        public async Task<MatchModel> AddMatchAsync(MatchModel match)
        {
            _context.Matches.Add(match);
            await _context.SaveChangesAsync();
            var created = await GetMatchByIdAsync(match.MatchId);
            return created ?? match;
        }

        public async Task UpdateMatchAsync(MatchModel match)
        {
            _context.Matches.Update(match);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMatchAsync(int matchId)
        {
            var match = await _context.Matches.FindAsync(matchId);
            if (match != null)
            {
                _context.Matches.Remove(match);
                await _context.SaveChangesAsync();
            }
        }
    }
}
