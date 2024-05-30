using Microsoft.EntityFrameworkCore;
using SpeedwayTyperApp.Server.DbContexts;
using SpeedwayTyperApp.Server.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public class MatchRepository : IMatchRepository
    {
        private readonly TypingContext _context;

        public MatchRepository(TypingContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Match>> GetAllMatchesAsync()
        {
            return await _context.Matches.Include(m => m.HostTeam).Include(m => m.GuestTeam).ToListAsync();
        }

        public async Task<Match> GetMatchByIdAsync(int matchId)
        {
            return await _context.Matches.Include(m => m.HostTeam).Include(m => m.GuestTeam)
                                         .FirstOrDefaultAsync(m => m.MatchId == matchId);
        }

        public async Task AddMatchAsync(Match match)
        {
            _context.Matches.Add(match);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMatchAsync(Match match)
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
