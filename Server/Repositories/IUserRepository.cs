﻿using SpeedwayTyperApp.Shared.Models;

namespace SpeedwayTyperApp.Server.Repositories
{
    public interface IUserRepository
    {
        Task<UserModel> GetUserByIdAsync(string userId);
        Task UpdateUserAsync(UserModel user);
    }

}
