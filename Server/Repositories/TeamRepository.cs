using Microsoft.EntityFrameworkCore;
using SpeedwayTyperApp.Server.DbContexts;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly TypingContext _context;

        public TeamRepository(TypingContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TeamModel>> GetTeamsAsync()
        {
            return await _context.Teams
                                 .OrderBy(t => t.Name)
                                 .ToListAsync();
        }
    }
}
