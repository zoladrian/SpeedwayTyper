using System;
using System.ComponentModel.DataAnnotations;

namespace SpeedwayTyperApp.Shared.Models
{
    public class InviteCreateModel
    {
        [Required]
        [MaxLength(64)]
        public string Code { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int MaxUses { get; set; }

        public DateTime? ExpirationDate { get; set; }
    }
}
