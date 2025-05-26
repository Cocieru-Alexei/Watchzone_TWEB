using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using WatchZone.BusinessLogic.Database;
using WatchZone.BusinessLogic.Interface;
using WatchZone.Domain.Entities.User;
using WatchZone.Domain.Enums;

namespace WatchZone.BusinessLogic.Repository
{
    public class UserRepository : IUserRepository
    {
        public async Task<IEnumerable<UserMinimal>> GetAllUsersAsync()
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

        public async Task<UserMinimal> GetUserByIdAsync(int id)
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

        public async Task<UserMinimal> GetUserByUsernameAsync(string username)
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

        public async Task<UserMinimal> GetUserByCredentialAsync(string credential)
        {
            using (var context = new UserContext())
            {
                var user = await Task.FromResult(context.Users.FirstOrDefault(u => u.Username == credential || u.Email == credential));
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

        public async Task<bool> UpdateUserAsync(UserMinimal user)
        {
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
                    return true;
                }
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            using (var context = new UserContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == id);
                if (user != null)
                {
                    context.Users.Remove(user);
                    await Task.FromResult(context.SaveChanges());
                    return true;
                }
                return false;
            }
        }

        public async Task<bool> UserExistsAsync(string username, string email)
        {
            using (var context = new UserContext())
            {
                return await Task.FromResult(context.Users.Any(u => u.Username == username || u.Email == email));
            }
        }

        public async Task<UDbTable> AuthenticateUserAsync(string credential, string hashedPassword)
        {
            using (var context = new UserContext())
            {
                return await Task.FromResult(context.Users.FirstOrDefault(u => 
                    (u.Username == credential || u.Email == credential) &&
                    u.Password == hashedPassword));
            }
        }

        public async Task<UDbTable> CreateUserAsync(UDbTable user)
        {
            using (var context = new UserContext())
            {
                var addedUser = context.Users.Add(user);
                await Task.FromResult(context.SaveChanges());
                return addedUser;
            }
        }

        public async Task<bool> UpdateUserLoginInfoAsync(int userId, DateTime loginTime, string ipAddress)
        {
            using (var context = new UserContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    user.LastLogin = loginTime;
                    user.LasIp = ipAddress;
                    await Task.FromResult(context.SaveChanges());
                    return true;
                }
                return false;
            }
        }

        public async Task<bool> UpdateUserPasswordAsync(int userId, string hashedPassword)
        {
            using (var context = new UserContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    user.Password = hashedPassword;
                    await Task.FromResult(context.SaveChanges());
                    return true;
                }
                return false;
            }
        }

        public async Task<string> GetUserPasswordAsync(int userId)
        {
            using (var context = new UserContext())
            {
                var user = await Task.FromResult(context.Users.FirstOrDefault(u => u.Id == userId));
                return user?.Password;
            }
        }

        public async Task<bool> VerifyUserPasswordAsync(int userId, string hashedPassword)
        {
            using (var context = new UserContext())
            {
                var user = await Task.FromResult(context.Users.FirstOrDefault(u => u.Id == userId));
                return user?.Password == hashedPassword;
            }
        }
    }
} 