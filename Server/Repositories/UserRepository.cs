﻿using SpeedwayTyperApp.Server.DbContexts;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly TypingContext _context;

        public UserRepository(TypingContext context)
        {
            _context = context;
        }

        public async Task<UserModel> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task UpdateUserAsync(UserModel user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }

}
