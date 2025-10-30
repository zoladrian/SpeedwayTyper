using System.Collections.Generic;
using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserModel>> GetAllUsersAsync();
        Task<UserModel> GetUserByIdAsync(string userId);
        Task<IEnumerable<UserModel>> GetAllUsersAsync();
        Task UpdateUserAsync(UserModel user);
        Task<List<UserModel>> GetPendingUsersAsync();
    }
}
