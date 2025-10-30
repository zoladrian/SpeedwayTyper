using System.Collections.Generic;
using System.Threading.Tasks;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserModel>> GetAllUsersAsync();
        Task<UserModel?> GetUserByIdAsync(string userId);
        Task UpdateUserAsync(UserModel user);
        Task<List<UserModel>> GetPendingUsersAsync();
    }
}
