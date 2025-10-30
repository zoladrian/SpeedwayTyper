using System.Text.Json.Serialization;

namespace SpeedwayTyperApp.Shared.Models
{
    public class MatchModel
    {
        public int MatchId { get; set; }
        public int SeasonId { get; set; }
        public SeasonModel? Season { get; set; }
        public int RoundId { get; set; }
        public RoundModel? Round { get; set; }
        public int HostTeamId { get; set; }
        public TeamModel? HostTeam { get; set; }
        public int GuestTeamId { get; set; }
        public TeamModel? GuestTeam { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
        public int? HostScore { get; set; }
        public int? GuestScore { get; set; }
        public DateTime? RescheduledFromUtc { get; set; }
        public DateTime? RescheduledToUtc { get; set; }
        public string? RescheduleReason { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MatchStatus
    {
        Scheduled = 0,
        Live = 1,
        Completed = 2,
        Postponed = 3,
        Cancelled = 4
    }
}
