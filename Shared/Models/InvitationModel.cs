using System.ComponentModel.DataAnnotations;

namespace SpeedwayTyperApp.Shared.Models
{
    public enum InvitationStatus
    {
        Pending,
        Accepted,
        Declined,
        Expired
    }

    public class InvitationModel
    {
        public int InvitationId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? SenderId { get; set; }

        public string? RecipientId { get; set; }

        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }
    }
}
