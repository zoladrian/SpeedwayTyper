using SpeedwayTyperApp.Server.Repositories;
using SpeedwayTyperApp.Shared.Models;
using System;
using System.Diagnostics.Eventing.Reader;

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
            if (match == null || !match.IsCompleted)
            {
                return 0;
            }

            var predictedDifference = Math.Abs(prediction.HostTeamPredictedScore - prediction.GuestTeamPredictedScore);
            var actualDifference = Math.Abs(match.HostTeamScore.Value - match.GuestTeamScore.Value);
            bool drawResult = match.HostTeamScore.Value == match.GuestTeamScore.Value;
            int points = 0;

            if (predictedDifference == actualDifference || drawResult)
            {
                if (match.HostTeamScore.Value + match.GuestTeamScore.Value == 0)
                {
                    points = 20;
                    return points;
                }
                else
                {
                    if (predictedDifference == 0 && actualDifference == 0)
                    {
                        points = 50;
                        prediction.AccurateResult = true;
                    }
                    else if (prediction.HostTeamPredictedScore == match.HostTeamScore &&
                             prediction.GuestTeamPredictedScore == match.GuestTeamScore)
                    {
                        points = 35;
                        prediction.AccurateResult = true;
                    }
                }
            }
            else if ((prediction.HostTeamPredictedScore > prediction.GuestTeamPredictedScore &&
                 match.HostTeamScore < match.GuestTeamScore) ||
                (prediction.HostTeamPredictedScore < prediction.GuestTeamPredictedScore &&
                 match.HostTeamScore > match.GuestTeamScore) ||
                 (prediction.HostTeamPredictedScore == prediction.GuestTeamPredictedScore && !drawResult))
            {
                points = 0;
            }

            else
            {
                int difference = Math.Abs(predictedDifference - actualDifference);

                if (difference <= 2)
                    points = 20;
                else if (difference <= 4)
                    points = 18;
                else if (difference <= 6)
                    points = 16;
                else if (difference <= 8)
                    points = 14;
                else if (difference <= 10)
                    points = 12;
                else if (difference <= 12)
                    points = 10;
                else if (difference <= 14)
                    points = 8;
                else if (difference <= 16)
                    points = 6;
                else if (difference <= 18)
                    points = 4;
                else
                    points = 2;
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
            user.TotalPoints = totalPoints;
            user.AccurateMatchResults = totalAccurateResults;
            await _userRepository.UpdateUserAsync(user);
        }
    }
}
