using Microsoft.EntityFrameworkCore;
using SpeedwayTyperApp.Server.DbContexts;
using SpeedwayTyperApp.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeedwayTyperApp.Server.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly TypingContext _context;

        public UserRepository(TypingContext context)
        {
            _context = context;
        }

        public async Task<List<UserModel>> GetPendingUsersAsync()
        {
            return await _context.Users
                .Where(u => u.IsPendingApproval)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<UserModel?> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<IEnumerable<UserModel>> GetAllUsersAsync()
        {
            return await _context.Users
                                  .OrderByDescending(u => u.TotalPoints)
                                  .ThenBy(u => u.UserName)
                                  .ToListAsync();
        }

        public async Task UpdateUserAsync(UserModel user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
