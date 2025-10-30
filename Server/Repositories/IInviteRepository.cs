using System.Collections.Generic;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public interface IInviteRepository
    {
        Task<InviteCodeModel?> GetByCodeAsync(string code);
        Task<IEnumerable<InviteCodeModel>> GetAllAsync();
        Task AddAsync(InviteCodeModel inviteCode);
        Task UpdateAsync(InviteCodeModel inviteCode);
    }
}
