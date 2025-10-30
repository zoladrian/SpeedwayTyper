namespace SpeedwayTyperApp.Shared.Models
{
    public class PredictionModel
    {
        public int PredictionId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public UserModel? User { get; set; }
        public int MatchId { get; set; }
        public MatchModel? Match { get; set; }
        public int HostTeamPredictedScore { get; set; }
        public int GuestTeamPredictedScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PointsAwarded { get; set; }
        public bool AccurateResult { get; set; }
    }
}
