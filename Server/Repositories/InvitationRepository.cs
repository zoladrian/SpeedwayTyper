using Microsoft.EntityFrameworkCore;
using SpeedwayTyperApp.Server.DbContexts;
using SpeedwayTyperApp.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeedwayTyperApp.Server.Repositories
{
    public class InvitationRepository : IInvitationRepository
    {
        private readonly TypingContext _context;

        public InvitationRepository(TypingContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InvitationModel>> GetInvitationsAsync()
        {
            return await _context.Invitations
                                 .OrderByDescending(i => i.CreatedAt)
                                 .ToListAsync();
        }

        public async Task<InvitationModel?> GetInvitationByIdAsync(int invitationId)
        {
            return await _context.Invitations.FindAsync(invitationId);
        }

        public async Task AddInvitationAsync(InvitationModel invitation)
        {
            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateInvitationAsync(InvitationModel invitation)
        {
            _context.Invitations.Update(invitation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteInvitationAsync(int invitationId)
        {
            var invitation = await _context.Invitations.FindAsync(invitationId);
            if (invitation != null)
            {
                _context.Invitations.Remove(invitation);
                await _context.SaveChangesAsync();
            }
        }
    }
}
