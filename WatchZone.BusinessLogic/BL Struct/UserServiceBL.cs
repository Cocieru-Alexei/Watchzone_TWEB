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

        public UserServiceBL(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public async Task<IEnumerable<UserMinimal>> GetAllUsersAsync()
        {
            try
            {
                using (var context = new UserContext())
                {
                    return await Task.FromResult(context.Users.Select(u => new UserMinimal
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                        LastLogin = u.LastLogin,
                        LasIp = u.LasIp,
                        Level = u.Level
                    }).ToList());
                }
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
                using (var context = new UserContext())
                {
                    var user = await Task.FromResult(context.Users.FirstOrDefault(u => u.Id == id));
                    if (user != null)
                    {
                        return new UserMinimal
                        {
                            Id = user.Id,
                            Username = user.Username,
                            Email = user.Email,
                            LastLogin = user.LastLogin,
                            LasIp = user.LasIp,
                            Level = user.Level
                        };
                    }
                    return null;
                }
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
                using (var context = new UserContext())
                {
                    var user = await Task.FromResult(context.Users.FirstOrDefault(u => u.Username == username || u.Email == username));
                    if (user != null)
                    {
                        return new UserMinimal
                        {
                            Id = user.Id,
                            Username = user.Username,
                            Email = user.Email,
                            LastLogin = user.LastLogin,
                            LasIp = user.LasIp,
                            Level = user.Level
                        };
                    }
                    return null;
                }
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

                using (var context = new UserContext())
                {
                    var existingUser = context.Users.FirstOrDefault(u => u.Id == user.Id);
                    if (existingUser != null)
                    {
                        existingUser.Username = user.Username;
                        existingUser.Email = user.Email;
                        existingUser.Level = user.Level;
                        existingUser.LastLogin = user.LastLogin;
                        existingUser.LasIp = user.LasIp;

                        await Task.FromResult(context.SaveChanges());
                        _errorHandler.LogInfo($"Updated user: {user.Username} (ID: {user.Id})");
                        return true;
                    }
                    
                    _errorHandler.LogWarning($"User not found for update: ID {user.Id}");
                    return false;
                }
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
                using (var context = new UserContext())
                {
                    var user = context.Users.FirstOrDefault(u => u.Id == id);
                    if (user != null)
                    {
                        context.Users.Remove(user);
                        await Task.FromResult(context.SaveChanges());
                        _errorHandler.LogInfo($"Deleted user with ID: {id}");
                        return true;
                    }
                    
                    _errorHandler.LogWarning($"User not found for deletion: ID {id}");
                    return false;
                }
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
                using (var context = new UserContext())
                {
                    return await Task.FromResult(context.Users.Any(u => u.Username == username || u.Email == email));
                }
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error checking if user exists: {username}, {email}");
                return true; // Assume exists to be safe
            }
        }
    }
} 