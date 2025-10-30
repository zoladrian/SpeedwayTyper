using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Shared.Models;
using System;
using System.Linq;

namespace SpeedwayTyperApp.Server.Services
{
    public class InviteService : IInviteService
    {
        private readonly IInviteRepository _inviteRepository;
        private readonly UserManager<UserModel> _userManager;

        public InviteService(IInviteRepository inviteRepository, UserManager<UserModel> userManager)
        {
            _inviteRepository = inviteRepository;
            _userManager = userManager;
        }

        public async Task<InviteCodeModel> CreateInviteAsync(InviteCreateModel model, string createdById, string createdByUserName)
        {
            var normalizedCode = NormalizeCode(model.Code);
            var existing = await _inviteRepository.GetByCodeAsync(normalizedCode);
            if (existing != null)
            {
                throw new InvalidOperationException("Invite code already exists.");
            }

            var invite = new InviteCodeModel
            {
                Code = normalizedCode,
                MaxUses = model.MaxUses,
                Uses = 0,
                ExpirationDate = model.ExpirationDate?.ToUniversalTime(),
                CreatedById = createdById,
                CreatedByUserName = createdByUserName
            };

            await _inviteRepository.AddAsync(invite);
            return invite;
        }

        public async Task<IEnumerable<InviteCodeModel>> GetInvitesAsync()
        {
            var invites = (await _inviteRepository.GetAllAsync()).ToList();
            var creatorIds = invites.Select(i => i.CreatedById).Distinct().ToList();
            if (creatorIds.Count == 0)
            {
                return invites;
            }

            var creators = await _userManager.Users
                .Where(u => creatorIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            foreach (var invite in invites)
            {
                invite.CreatedByUserName = creators.FirstOrDefault(c => c.Id == invite.CreatedById)?.UserName;
            }

            return invites;
        }

        public async Task<InviteValidationResult> ValidateInviteAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return new InviteValidationResult(false, "Kod zaproszenia jest wymagany.");
            }

            var normalizedCode = NormalizeCode(code);
            var invite = await _inviteRepository.GetByCodeAsync(normalizedCode);
            if (invite == null)
            {
                return new InviteValidationResult(false, "Podany kod zaproszenia nie istnieje.");
            }

            var validationMessage = ValidateInvite(invite);
            if (validationMessage != null)
            {
                return new InviteValidationResult(false, validationMessage, invite);
            }

            return new InviteValidationResult(true, null, invite);
        }

        public async Task<InviteValidationResult> TryConsumeInviteAsync(string code)
        {
            var validation = await ValidateInviteAsync(code);
            if (!validation.Success || validation.Invite == null)
            {
                return validation;
            }

            var invite = validation.Invite;
            invite.Uses += 1;
            await _inviteRepository.UpdateAsync(invite);
            return new InviteValidationResult(true, null, invite);
        }

        private static string NormalizeCode(string code)
        {
            return code.Trim().ToUpperInvariant();
        }

        private static string? ValidateInvite(InviteCodeModel invite)
        {
            if (invite.ExpirationDate.HasValue && invite.ExpirationDate.Value < DateTime.UtcNow)
            {
                return "Kod zaproszenia wygasł.";
            }

            if (invite.Uses >= invite.MaxUses)
            {
                return "Kod zaproszenia został już wykorzystany maksymalną liczbę razy.";
            }

            return null;
        }
    }
}
