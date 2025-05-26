using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WatchZone.BusinessLogic.Interface;
using WatchZone.BusinessLogic.Database;
using WatchZone.Domain.Entities.User;

namespace WatchZone.BusinessLogic.BL_Struct
{
    public class UserServiceBL : IUserService
    {
        private readonly IErrorHandler _errorHandler;
        private readonly IUserRepository _userRepository;

        public UserServiceBL(IErrorHandler errorHandler, IUserRepository userRepository)
        {
            _errorHandler = errorHandler;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserMinimal>> GetAllUsersAsync()
        {
            try
            {
                return await _userRepository.GetAllUsersAsync();
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "Error getting all users");
                return Enumerable.Empty<UserMinimal>();
            }
        }

        public async Task<UserMinimal> GetUserByIdAsync(int id)
        {
            try
            {
                return await _userRepository.GetUserByIdAsync(id);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error getting user with ID: {id}");
                return null;
            }
        }

        public async Task<UserMinimal> GetUserByUsernameAsync(string username)
        {
            try
            {
                return await _userRepository.GetUserByUsernameAsync(username);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error getting user with username: {username}");
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(UserMinimal user)
        {
            try
            {
                if (user == null)
                {
                    _errorHandler.LogWarning("Attempted to update null user");
                    return false;
                }

                var result = await _userRepository.UpdateUserAsync(user);
                if (result)
                {
                    _errorHandler.LogInfo($"Updated user: {user.Username} (ID: {user.Id})");
                }
                else
                {
                    _errorHandler.LogWarning($"User not found for update: ID {user.Id}");
                }
                return result;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error updating user: {user?.Username} (ID: {user?.Id})");
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var result = await _userRepository.DeleteUserAsync(id);
                if (result)
                {
                    _errorHandler.LogInfo($"Deleted user with ID: {id}");
                }
                else
                {
                    _errorHandler.LogWarning($"User not found for deletion: ID {id}");
                }
                return result;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error deleting user with ID: {id}");
                return false;
            }
        }

        public async Task<bool> UserExistsAsync(string username, string email)
        {
            try
            {
                return await _userRepository.UserExistsAsync(username, email);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error checking if user exists: {username}, {email}");
                return true; // Assume exists to be safe
            }
        }

        public bool VerifyPassword(int userId, string plainPassword)
        {
            try
            {
                // All passwords are now consistently hashed with SHA256
                var hashedPassword = WatchZone.Helper.LoginUtility.GenHash(plainPassword);
                var result = _userRepository.VerifyUserPasswordAsync(userId, hashedPassword).Result;
                
                if (!result)
                {
                    _errorHandler.LogWarning($"Password verification failed for user ID: {userId}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error verifying password for user ID: {userId}");
                return false;
            }
        }

        public bool UpdatePassword(int userId, string newPlainPassword)
        {
            try
            {
                // All passwords are now consistently hashed with SHA256
                var hashedPassword = WatchZone.Helper.LoginUtility.GenHash(newPlainPassword);
                var result = _userRepository.UpdateUserPasswordAsync(userId, hashedPassword).Result;
                
                if (result)
                {
                    _errorHandler.LogInfo($"Password updated for user ID: {userId}");
                }
                else
                {
                    _errorHandler.LogWarning($"User not found for password update: ID {userId}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error updating password for user ID: {userId}");
                return false;
            }
        }
    }
} 