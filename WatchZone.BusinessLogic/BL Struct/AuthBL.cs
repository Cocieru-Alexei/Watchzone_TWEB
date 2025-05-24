using System;
using System.Linq;
using System.Web;
using WatchZone.BusinessLogic.Interface;
using WatchZone.BusinessLogic.Database;
using WatchZone.BusinessLogic.Core;
using WatchZone.Domain.Entities.User;
using WatchZone.Domain.Enums;
using WatchZone.Domain.Model;
using WatchZone.Domain.Model.User;
using WatchZone.Helper;

namespace WatchZone.BusinessLogic.BL_Struct
{
    public class AuthBL : UserApi, IAuth
    {
        private readonly IErrorHandler _errorHandler;

        public AuthBL(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public string UserAuthLogic(UserLoginDTO data)
        {
            return UserAuthLogicAction(data, _errorHandler);
        }

        public ULoginResp UserLogin(ULoginData data)
        {
            try
            {
                using (var context = new UserContext())
                {
                    var user = context.Users.FirstOrDefault(u => 
                        (u.Username == data.Credential || u.Email == data.Credential) &&
                        u.Password == data.Password);

                    if (user != null)
                    {
                        // Update last login info
                        user.LastLogin = data.LoginDateTime;
                        user.LasIp = data.LoginIp;
                        context.SaveChanges();

                        _errorHandler.LogInfo($"User {data.Credential} logged in successfully");
                        return new ULoginResp { Status = true, StatusMsg = "Login successful" };
                    }

                    _errorHandler.LogWarning($"Failed login attempt for {data.Credential}");
                    return new ULoginResp { Status = false, StatusMsg = "Invalid credentials" };
                }
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error during login for {data.Credential}");
                return new ULoginResp { Status = false, StatusMsg = "Login error occurred" };
            }
        }

        public URegisterResp UserRegister(URegisterData data)
        {
            try
            {
                using (var context = new UserContext())
                {
                    // Check if user exists
                    if (context.Users.Any(u => u.Username == data.Credential))
                    {
                        return new URegisterResp { Status = false, StatusMsg = "User already exists" };
                    }

                    var newUser = new UDbTable
                    {
                        Username = data.Credential,
                        Email = data.Credential, // Since URegisterData doesn't have Email, use Credential
                        Password = data.Password,
                        Level = URole.User,
                        LastLogin = data.RegisterDateTime,
                        LasIp = data.RegisterIp
                    };

                    context.Users.Add(newUser);
                    context.SaveChanges();

                    _errorHandler.LogInfo($"New user registered: {data.Credential}");
                    return new URegisterResp { Status = true, StatusMsg = "Registration successful" };
                }
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error during registration for {data.Credential}");
                return new URegisterResp { Status = false, StatusMsg = "Registration error occurred" };
            }
        }

        public HttpCookie GenerateCookie(string loginCredential)
        {
            try
            {
                var cookieValue = CookieUtility.Create(loginCredential);
                var cookie = new HttpCookie("X-KEY", cookieValue)
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = true
                };
                return cookie;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error generating cookie for {loginCredential}");
                return null;
            }
        }

        public UserMinimal GetUserByCookie(string cookieValue)
        {
            try
            {
                var username = CookieUtility.Validate(cookieValue);
                if (string.IsNullOrEmpty(username))
                    return null;

                using (var context = new UserContext())
                {
                    var user = context.Users.FirstOrDefault(u => u.Username == username || u.Email == username);
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
                }
                return null;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "Error getting user by cookie");
                return null;
            }
        }

        public new URole? GetUserRoleByCookie(string cookieValue)
        {
            try
            {
                var user = GetUserByCookie(cookieValue);
                return user?.Level;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "Error getting user role by cookie");
                return null;
            }
        }

        public void ExpireUserSession(string cookieValue)
        {
            try
            {
                // In a real implementation, you might invalidate the session in the database
                _errorHandler.LogInfo("User session expired");
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "Error expiring user session");
            }
        }

        public bool ValidateUserAccess(string cookieValue, URole requiredRole = URole.User)
        {
            try
            {
                var userRole = GetUserRoleByCookie(cookieValue);
                if (!userRole.HasValue)
                    return false;

                // Admin can access everything
                if (userRole.Value == URole.Admin)
                    return true;

                // Check if user has required role
                return userRole.Value >= requiredRole;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "Error validating user access");
                return false;
            }
        }

        public int? GetCurrentUserId(string cookieValue)
        {
            try
            {
                var user = GetUserByCookie(cookieValue);
                return user?.Id;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "Error getting current user ID");
                return null;
            }
        }
    }
} 