using Microsoft.EntityFrameworkCore;
using SpeedwayTyperApp.Server.DbContexts;
using SpeedwayTyperApp.Shared.Models;
using System.Collections.Generic;
using System.Linq;

namespace SpeedwayTyperApp.Server.Services
{
    public class MatchService : IMatchService
    {
        private readonly TypingContext _context;
        private readonly IPredictionService _predictionService;

        public MatchService(TypingContext context, IPredictionService predictionService)
        {
            _context = context;
            _predictionService = predictionService;
        }

        public async Task UpdateMatchAsync(MatchModel match)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingMatch = await _context.Matches.FirstOrDefaultAsync(m => m.MatchId == match.MatchId);
                if (existingMatch == null)
                {
                    throw new KeyNotFoundException($"Match with id {match.MatchId} was not found.");
                }

                existingMatch.Date = match.Date;
                existingMatch.Round = match.Round;
                existingMatch.HostTeamId = match.HostTeamId;
                existingMatch.GuestTeamId = match.GuestTeamId;
                existingMatch.HostTeamScore = match.HostTeamScore;
                existingMatch.GuestTeamScore = match.GuestTeamScore;
                existingMatch.IsCompleted = match.IsCompleted;

                await _context.SaveChangesAsync();

                if (existingMatch.IsCompleted && existingMatch.HostTeamScore.HasValue && existingMatch.GuestTeamScore.HasValue)
                {
                    var predictions = await _context.Predictions
                                                    .Where(p => p.MatchId == existingMatch.MatchId)
                                                    .ToListAsync();

                    var affectedUserIds = new HashSet<string>();

                    foreach (var prediction in predictions)
                    {
                        prediction.PointsAwarded = await _predictionService.CalculatePointsAsync(prediction);
                        affectedUserIds.Add(prediction.UserId);
                    }

                    await _context.SaveChangesAsync();

                    foreach (var userId in affectedUserIds)
                    {
                        await _predictionService.UpdateUserPointsAsync(userId);
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
