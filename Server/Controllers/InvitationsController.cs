using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Shared.Models;
using System.Security.Claims;

namespace SpeedwayTyperApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class InvitationsController : ControllerBase
    {
        private readonly IInvitationRepository _invitationRepository;

        public InvitationsController(IInvitationRepository invitationRepository)
        {
            _invitationRepository = invitationRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvitationModel>>> GetInvitations()
        {
            var invitations = await _invitationRepository.GetInvitationsAsync();
            return Ok(invitations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InvitationModel>> GetInvitation(int id)
        {
            var invitation = await _invitationRepository.GetInvitationByIdAsync(id);
            if (invitation == null)
            {
                return NotFound();
            }

            return Ok(invitation);
        }

        [HttpPost]
        public async Task<ActionResult<InvitationModel>> CreateInvitation([FromBody] InvitationModel invitation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            invitation.InvitationId = 0;
            invitation.SenderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            invitation.Status = InvitationStatus.Pending;
            invitation.CreatedAt = DateTime.UtcNow;
            await _invitationRepository.AddInvitationAsync(invitation);

            return CreatedAtAction(nameof(GetInvitation), new { id = invitation.InvitationId }, invitation);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateInvitationStatus(int id, [FromBody] InvitationStatusUpdateRequest request)
        {
            var invitation = await _invitationRepository.GetInvitationByIdAsync(id);
            if (invitation == null)
            {
                return NotFound();
            }

            invitation.Status = request.Status;
            invitation.RespondedAt = request.Status == InvitationStatus.Pending ? null : DateTime.UtcNow;
            await _invitationRepository.UpdateInvitationAsync(invitation);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvitation(int id)
        {
            await _invitationRepository.DeleteInvitationAsync(id);
            return NoContent();
        }

        public class InvitationStatusUpdateRequest
        {
            public InvitationStatus Status { get; set; }
        }
    }
}
