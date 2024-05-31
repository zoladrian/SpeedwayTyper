﻿using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(string userId);
        Task UpdateUserAsync(User user);
    }

}
