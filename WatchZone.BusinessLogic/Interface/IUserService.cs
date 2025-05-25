using System.Collections.Generic;
using System.Threading.Tasks;
using WatchZone.Domain.Entities.User;

namespace WatchZone.BusinessLogic.Interface
{
    public interface IUserService
    {
        Task<IEnumerable<UserMinimal>> GetAllUsersAsync();
        Task<UserMinimal> GetUserByIdAsync(int id);
        Task<UserMinimal> GetUserByUsernameAsync(string username);
        Task<bool> UpdateUserAsync(UserMinimal user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> UserExistsAsync(string username, string email);
        bool VerifyPassword(int userId, string plainPassword);
        bool UpdatePassword(int userId, string newPlainPassword);
    }
} 