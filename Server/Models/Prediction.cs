namespace SpeedwayTyperApp.Server.Models
{
    public class Prediction
    {
        public int PredictionId { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public int MatchId { get; set; }
        public Match Match { get; set; }
        public int HostTeamPredictedScore { get; set; }
        public int GuestTeamPredictedScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PointsAwarded { get; set; }
        public bool AccurateResult { get; set; }
    }
}
