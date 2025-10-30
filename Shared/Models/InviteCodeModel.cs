using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpeedwayTyperApp.Shared.Models
{
    public class InviteCodeModel
    {
        public int InviteCodeId { get; set; }

        [Required]
        [MaxLength(64)]
        public string Code { get; set; }

        [Range(1, int.MaxValue)]
        public int MaxUses { get; set; }

        public int Uses { get; set; }

        public DateTime? ExpirationDate { get; set; }

        [Required]
        public string CreatedById { get; set; }

        [NotMapped]
        public string? CreatedByUserName { get; set; }

        [NotMapped]
        public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;
    }
}
