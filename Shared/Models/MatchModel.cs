namespace SpeedwayTyperApp.Shared.Models
{
    public class MatchModel
    {
        public int MatchId { get; set; }
        public DateTime Date { get; set; }
        public int Round { get; set; }
        public TeamModel HostTeam { get; set; } = null!;
        public int HostTeamId {  get; set; }
        public TeamModel GuestTeam { get; set; } = null!;
        public int GuestTeamId { get; set; }
        public int? HostTeamScore { get; set; }
        public int? GuestTeamScore { get; set; }
        public bool IsCompleted { get; set; }
    }
}
