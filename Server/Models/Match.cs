namespace SpeedwayTyperApp.Server.Models
{
    public class Match
    {
        public int MatchId { get; set; }
        public DateTime Date { get; set; }
        public int Round { get; set; }
        public Team HostTeam { get; set; }
        public int HostTeamId {  get; set; }
        public Team GuestTeam { get; set; }
        public int GuestTeamId { get; set; }
        public int? HostTeamScore { get; set; }
        public int? GuestTeamScore { get; set; }
        public bool IsCompleted { get; set; }
    }
}
