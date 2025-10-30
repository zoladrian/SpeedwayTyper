using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public interface IInvitationRepository
    {
        Task<IEnumerable<InvitationModel>> GetInvitationsAsync();
        Task<InvitationModel?> GetInvitationByIdAsync(int invitationId);
        Task AddInvitationAsync(InvitationModel invitation);
        Task UpdateInvitationAsync(InvitationModel invitation);
        Task DeleteInvitationAsync(int invitationId);
    }
}
