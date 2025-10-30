using System.ComponentModel.DataAnnotations;

namespace SpeedwayTyperApp.Shared.Models
{
    public class RoundModel
    {
        public int RoundId { get; set; }

        public int SeasonId { get; set; }

        public SeasonModel? Season { get; set; }

        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public int Order { get; set; }
    }
}
