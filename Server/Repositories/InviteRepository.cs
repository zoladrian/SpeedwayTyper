using Microsoft.EntityFrameworkCore;
using SpeedwayTyperApp.Server.DbContexts;
using SpeedwayTyperApp.Shared.Models;
using System.Collections.Generic;

namespace SpeedwayTyperApp.Server.Repositories
{
    public class InviteRepository : IInviteRepository
    {
        private readonly TypingContext _context;

        public InviteRepository(TypingContext context)
        {
            _context = context;
        }

        public async Task AddAsync(InviteCodeModel inviteCode)
        {
            _context.InviteCodes.Add(inviteCode);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<InviteCodeModel>> GetAllAsync()
        {
            return await _context.InviteCodes.AsNoTracking().ToListAsync();
        }

        public async Task<InviteCodeModel?> GetByCodeAsync(string code)
        {
            return await _context.InviteCodes.SingleOrDefaultAsync(i => i.Code == code);
        }

        public async Task UpdateAsync(InviteCodeModel inviteCode)
        {
            _context.InviteCodes.Update(inviteCode);
            await _context.SaveChangesAsync();
        }
    }
}
