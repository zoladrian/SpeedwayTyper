using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Shared.Models;
using System.Linq;

namespace SpeedwayTyperApp.Server.Services
{
    public class PredictionService : IPredictionService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPredictionRepository _predictionRepository;

        public PredictionService(IMatchRepository matchRepository, IUserRepository userRepository, IPredictionRepository predictionRepository)
        {
            _matchRepository = matchRepository;
            _userRepository = userRepository;
            _predictionRepository = predictionRepository;
        }

        public async Task<int> CalculatePointsAsync(PredictionModel prediction)
        {
            var match = await _matchRepository.GetMatchByIdAsync(prediction.MatchId);
            if (match == null || !match.IsCompleted || !match.HostTeamScore.HasValue || !match.GuestTeamScore.HasValue)
            {
                prediction.AccurateResult = false;
                return 0;
            }

            prediction.AccurateResult = false;

            var predictedDifference = Math.Abs(prediction.HostTeamPredictedScore - prediction.GuestTeamPredictedScore);
            var actualDifference = Math.Abs(match.HostTeamScore.Value - match.GuestTeamScore.Value);
            var predictionSum = prediction.HostTeamPredictedScore + prediction.GuestTeamPredictedScore;
            var resultSum = match.HostTeamScore.Value + match.GuestTeamScore.Value;

            var isPredictionDraw = prediction.HostTeamPredictedScore == prediction.GuestTeamPredictedScore;
            var isActualDraw = match.HostTeamScore.Value == match.GuestTeamScore.Value;
            var isTypicalPrediction = predictionSum == 90;
            var isTypicalResult = resultSum == 90;

            if (isPredictionDraw && !isActualDraw)
            {
                return 0;
            }

            if (!isPredictionDraw && isActualDraw)
            {
                return 0;
            }

            if (isPredictionDraw && isActualDraw && isTypicalPrediction && isTypicalResult &&
                prediction.HostTeamPredictedScore == match.HostTeamScore &&
                prediction.GuestTeamPredictedScore == match.GuestTeamScore)
            {
                prediction.AccurateResult = true;
                return 50;
            }

            if (!isPredictionDraw && !isActualDraw && isTypicalPrediction && isTypicalResult &&
                prediction.HostTeamPredictedScore == match.HostTeamScore &&
                prediction.GuestTeamPredictedScore == match.GuestTeamScore)
            {
                prediction.AccurateResult = true;
                return 35;
            }

            var difference = Math.Abs(predictedDifference - actualDifference);

            int points = difference switch
            {
                <= 2 => 20,
                <= 4 => 18,
                <= 6 => 16,
                <= 8 => 14,
                <= 10 => 12,
                <= 12 => 10,
                <= 14 => 8,
                <= 16 => 6,
                <= 18 => 4,
                _ => 2
            };

            if (isPredictionDraw && isActualDraw && !isTypicalResult)
            {
                points = Math.Min(points, 20);
            }

            if (!isActualDraw)
            {
                var predictedHostWin = prediction.HostTeamPredictedScore > prediction.GuestTeamPredictedScore;
                var actualHostWin = match.HostTeamScore.Value > match.GuestTeamScore.Value;

                if (predictedHostWin != actualHostWin)
                {
                    return 0;
                }
            }

            return points;
        }

        public async Task AddPredictionAsync(PredictionModel prediction)
        {
            prediction.PointsAwarded = await CalculatePointsAsync(prediction);
            await _predictionRepository.AddPredictionAsync(prediction);
            await UpdateUserPointsAsync(prediction.UserId);
        }

        public async Task UpdatePredictionAsync(PredictionModel prediction)
        {
            prediction.PointsAwarded = await CalculatePointsAsync(prediction);
            await _predictionRepository.UpdatePredictionAsync(prediction);
            await UpdateUserPointsAsync(prediction.UserId);
        }

        public async Task UpdateUserPointsAsync(string userId)
        {
            var predictions = await _predictionRepository.GetPredictionsByUserAsync(userId);
            var totalPoints = predictions.Sum(p => p.PointsAwarded);
            var totalAccurateResults = predictions.Count(a => a.AccurateResult);
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return;
            }

            user.TotalPoints = totalPoints;
            user.AccurateMatchResults = totalAccurateResults;
            await _userRepository.UpdateUserAsync(user);
        }
    }
}
