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
    public class AuthBL : IAuth
    {
        private readonly IErrorHandler _errorHandler;
        private readonly IUserRepository _userRepository;
        private readonly ISessionRepository _sessionRepository;

        public AuthBL(IErrorHandler errorHandler, IUserRepository userRepository, ISessionRepository sessionRepository)
        {
            _errorHandler = errorHandler;
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
        }

        public string UserAuthLogicWithErrorHandler(UserLoginDTO data)
        {
            return UserAuthLogic(data);
        }

        public string UserAuthLogic(UserLoginDTO data)
        {
            try
            {
                // All passwords are now consistently hashed with SHA256
                var hashedPassword = LoginUtility.GenHash(data.Password);
                
                var user = _userRepository.AuthenticateUserAsync(data.UserName, hashedPassword).Result;

                if (user != null)
                {
                    _errorHandler?.LogInfo($"User {data.UserName} authenticated successfully");
                    return "Authentication successful";
                }

                _errorHandler?.LogWarning($"Failed authentication attempt for {data.UserName}");
                return "Invalid credentials";
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, $"Error during authentication for {data.UserName}");
                return "Authentication error occurred";
            }
        }

        public ULoginResp UserLogin(ULoginData data)
        {
            try
            {
                // Hash the provided password for comparison
                var hashedPassword = LoginUtility.GenHash(data.Password);
                
                var user = _userRepository.AuthenticateUserAsync(data.Credential, hashedPassword).Result;

                if (user != null)
                {
                    // Update last login info
                    _userRepository.UpdateUserLoginInfoAsync(user.Id, data.LoginDateTime, data.LoginIp).Wait();

                    _errorHandler.LogInfo($"User {data.Credential} logged in successfully");
                    return new ULoginResp { Status = true, StatusMsg = "Login successful" };
                }

                _errorHandler.LogWarning($"Failed login attempt for {data.Credential}");
                return new ULoginResp { Status = false, StatusMsg = "Invalid credentials" };
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
                // Check if user exists
                if (_userRepository.UserExistsAsync(data.Credential, data.Credential).Result)
                {
                    _errorHandler.LogWarning($"Registration attempt for existing user: {data.Credential}");
                    return new URegisterResp { Status = false, StatusMsg = "User already exists" };
                }

                // Hash the password before storing
                var hashedPassword = LoginUtility.GenHash(data.Password);

                var newUser = new UDbTable
                {
                    Username = data.Credential,
                    Email = data.Credential, // Since URegisterData doesn't have Email, use Credential
                    Password = hashedPassword,
                    Level = URole.User,
                    LastLogin = data.RegisterDateTime,
                    LasIp = data.RegisterIp
                };

                var createdUser = _userRepository.CreateUserAsync(newUser).Result;
                
                if (createdUser != null)
                {
                    _errorHandler.LogInfo($"New user registered: {data.Credential}");
                    return new URegisterResp { Status = true, StatusMsg = "Registration successful" };
                }
                else
                {
                    return new URegisterResp { Status = false, StatusMsg = "Registration failed" };
                }
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error during registration for {data.Credential}: {ex.Message}");
                
                // Handle specific database schema issues
                if (ex.Message.Contains("String or binary data would be truncated") || 
                    ex.Message.Contains("model compatibility") ||
                    ex.Message.Contains("Invalid column"))
                {
                    return new URegisterResp { Status = false, StatusMsg = "Database schema issue. Please contact administrator." };
                }
                
                return new URegisterResp { Status = false, StatusMsg = "Registration failed. Please try again." };
            }
        }

        public HttpCookie GenerateCookie(string loginCredential)
        {
            try
            {
                var cookieValue = CookieUtility.Create(loginCredential);
                var expireTime = DateTime.Now.AddDays(30);
                
                // Create or update session using repository
                _sessionRepository.CreateOrUpdateSessionAsync(loginCredential, cookieValue, expireTime).Wait();
                
                var cookie = new HttpCookie("X-KEY", cookieValue)
                {
                    Expires = expireTime,
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
                // First try the new session-based approach
                var session = _sessionRepository.GetSessionByCookieAsync(cookieValue).Result;
                if (session != null)
                {
                    // Get user by username from session
                    return _userRepository.GetUserByUsernameAsync(session.Username).Result;
                }

                // Fallback to old cookie validation approach for backward compatibility
                var username = CookieUtility.Validate(cookieValue);
                if (!string.IsNullOrEmpty(username))
                {
                    return _userRepository.GetUserByUsernameAsync(username).Result;
                }

                return null;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "Error getting user by cookie");
                return null;
            }
        }

        public URole? GetUserRoleByCookie(string cookieValue)
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
                // Delete session from database using repository
                var result = _sessionRepository.DeleteSessionAsync(cookieValue).Result;
                if (result)
                {
                    _errorHandler.LogInfo("User session expired successfully");
                }
                else
                {
                    _errorHandler.LogWarning("Session not found for expiration");
                }
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