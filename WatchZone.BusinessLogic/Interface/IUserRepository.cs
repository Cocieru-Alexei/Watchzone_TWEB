using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WatchZone.Domain.Entities.User;
using WatchZone.Domain.Enums;

namespace WatchZone.BusinessLogic.Interface
{
    public interface IUserRepository
    {
        // User CRUD operations
        Task<IEnumerable<UserMinimal>> GetAllUsersAsync();
        Task<UserMinimal> GetUserByIdAsync(int id);
        Task<UserMinimal> GetUserByUsernameAsync(string username);
        Task<UserMinimal> GetUserByCredentialAsync(string credential);
        Task<bool> UpdateUserAsync(UserMinimal user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> UserExistsAsync(string username, string email);
        
        // Authentication operations
        Task<UDbTable> AuthenticateUserAsync(string credential, string hashedPassword);
        Task<UDbTable> CreateUserAsync(UDbTable user);
        Task<bool> UpdateUserLoginInfoAsync(int userId, DateTime loginTime, string ipAddress);
        Task<bool> UpdateUserPasswordAsync(int userId, string hashedPassword);
        
        // Password operations
        Task<string> GetUserPasswordAsync(int userId);
        Task<bool> VerifyUserPasswordAsync(int userId, string hashedPassword);
    }
} 