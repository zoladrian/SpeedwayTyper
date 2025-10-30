using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public interface ITeamRepository
    {
        Task<IEnumerable<TeamModel>> GetTeamsAsync();
    }
}
