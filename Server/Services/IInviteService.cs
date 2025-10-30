using System.Collections.Generic;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Services
{
    public interface IInviteService
    {
        Task<IEnumerable<InviteCodeModel>> GetInvitesAsync();
        Task<InviteCodeModel> CreateInviteAsync(InviteCreateModel model, string createdById, string createdByUserName);
        Task<InviteValidationResult> ValidateInviteAsync(string code);
        Task<InviteValidationResult> TryConsumeInviteAsync(string code);
    }
}
