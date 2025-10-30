using System.ComponentModel.DataAnnotations;

namespace SpeedwayTyperApp.Shared.Models
{
    public class RegisterByInviteModel
    {
        [Required]
        [MaxLength(256)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [MaxLength(64)]
        public string InviteCode { get; set; }
    }
}
