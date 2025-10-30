using System.ComponentModel.DataAnnotations;

namespace SpeedwayTyperApp.Shared.Models
{
    public class SeasonModel
    {
        public int SeasonId { get; set; }

        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public DateTime? StartDateUtc { get; set; }

        public DateTime? EndDateUtc { get; set; }

        public bool IsActive { get; set; }
    }
}
