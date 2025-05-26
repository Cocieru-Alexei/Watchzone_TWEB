using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WatchZone.Domain.Entities.User;
using WatchZone.Web.Models.Auth;
using WatchZone.Domain.Enums;
using AutoMapper;

namespace WatchZone.Web.Controllers
{
    public class AdminController : BaseController
    {
        public ActionResult Index()
        {
            // Use BaseController authentication/authorization
            var accessResult = RedirectIfNotAdmin();
            if (accessResult != null)
                return accessResult;

            try
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var userService = businessLogic.GetUserService();
                var authService = businessLogic.GetAuthService();
                var errorHandler = businessLogic.GetErrorHandler();

                // Log admin access
                var currentUser = authService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser != null)
                {
                    errorHandler.LogInfo($"Admin {currentUser.Username} (ID: {currentUser.Id}) accessed admin panel");
                }

                // Get data for tables using business logic services
                ViewBag.Users = GetUsers();

                return View("AdminPanel");
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Unable to load admin panel");
            }
        }

        [HttpGet]
        public JsonResult GetUser(int id)
        {
            // Use BaseController authentication/authorization
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Access denied" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var userService = businessLogic.GetUserService();
                var authService = businessLogic.GetAuthService();
                var errorHandler = businessLogic.GetErrorHandler();

                // Log admin action
                var currentUser = authService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser != null)
                {
                    errorHandler.LogInfo($"Admin {currentUser.Username} (ID: {currentUser.Id}) requested user details for ID: {id}");
                }

                // Use business logic UserService instead of direct database access
                var user = userService.GetUserByIdAsync(id).Result;
                if (user == null)
                {
                    errorHandler.LogWarning($"Admin requested non-existent user: ID {id}");
                    return Json(new { success = false, message = "User not found" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = user }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var errorHandler = businessLogic.GetErrorHandler();
                errorHandler.LogError(ex, $"Error getting user with ID: {id}");
                return Json(new { success = false, message = "Error retrieving user" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult AddUser(UserDataRegister userData)
        {
            // Use BaseController authentication/authorization
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Access denied" });
            }

            try
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var authService = businessLogic.GetAuthService();
                var errorHandler = businessLogic.GetErrorHandler();

                // Log admin action
                var currentUser = authService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser != null)
                {
                    errorHandler.LogInfo($"Admin {currentUser.Username} (ID: {currentUser.Id}) attempting to add user: {userData.UserName}");
                }

                var data = Mapper.Map<URegisterData>(userData);
                data.RegisterIp = Request.UserHostAddress;
                data.RegisterDateTime = DateTime.Now;

                // Use business logic AuthService instead of manual service creation
                var result = authService.UserRegister(data);
                if (result.Status)
                {
                    errorHandler.LogInfo($"Admin {currentUser?.Username} successfully added user: {userData.UserName}");
                }
                else
                {
                    errorHandler.LogWarning($"Admin {currentUser?.Username} failed to add user {userData.UserName}: {result.StatusMsg}");
                }
                return Json(new { success = result.Status, message = result.StatusMsg });
            }
            catch (Exception ex)
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var errorHandler = businessLogic.GetErrorHandler();
                errorHandler.LogError(ex, "Error adding user");
                return Json(new { success = false, message = "Error adding user" });
            }
        }

        [HttpPost]
        public JsonResult UpdateUser(UserMinimal userData)
        {
            // Use BaseController authentication/authorization
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Access denied" });
            }

            try
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var userService = businessLogic.GetUserService();
                var authService = businessLogic.GetAuthService();
                var errorHandler = businessLogic.GetErrorHandler();

                // Log admin action
                var currentUser = authService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser != null)
                {
                    errorHandler.LogInfo($"Admin {currentUser.Username} (ID: {currentUser.Id}) attempting to update user: {userData.Username} (ID: {userData.Id})");
                }

                // Use business logic UserService instead of direct database access
                var success = userService.UpdateUserAsync(userData).Result;
                if (success)
                {
                    errorHandler.LogInfo($"Admin {currentUser?.Username} successfully updated user: {userData.Username} (ID: {userData.Id})");
                    return Json(new { success = true, message = "User updated successfully" });
                }
                else
                {
                    errorHandler.LogWarning($"Admin {currentUser?.Username} failed to update user: {userData.Username} (ID: {userData.Id})");
                    return Json(new { success = false, message = "Failed to update user" });
                }
            }
            catch (Exception ex)
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var errorHandler = businessLogic.GetErrorHandler();
                errorHandler.LogError(ex, $"Error updating user with ID: {userData.Id}");
                return Json(new { success = false, message = "Error updating user" });
            }
        }

        [HttpPost]
        public JsonResult DeleteUser(int userId)
        {
            // Use BaseController authentication/authorization
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Access denied" });
            }

            try
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var userService = businessLogic.GetUserService();
                var authService = businessLogic.GetAuthService();
                var errorHandler = businessLogic.GetErrorHandler();

                // Log admin action
                var currentUser = authService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser != null)
                {
                    errorHandler.LogInfo($"Admin {currentUser.Username} (ID: {currentUser.Id}) attempting to delete user ID: {userId}");
                }

                // Get user details before deletion for logging
                var userToDelete = userService.GetUserByIdAsync(userId).Result;
                var userToDeleteName = userToDelete?.Username ?? $"Unknown (ID: {userId})";

                // Use business logic UserService instead of direct database access
                var success = userService.DeleteUserAsync(userId).Result;
                if (success)
                {
                    errorHandler.LogInfo($"Admin {currentUser?.Username} successfully deleted user: {userToDeleteName} (ID: {userId})");
                    return Json(new { success = true, message = "User deleted successfully" });
                }
                else
                {
                    errorHandler.LogWarning($"Admin {currentUser?.Username} failed to delete user: {userToDeleteName} (ID: {userId})");
                    return Json(new { success = false, message = "Failed to delete user" });
                }
            }
            catch (Exception ex)
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var errorHandler = businessLogic.GetErrorHandler();
                errorHandler.LogError(ex, $"Error deleting user with ID: {userId}");
                return Json(new { success = false, message = "Error deleting user" });
            }
        }





        #region Private Methods



        private List<UserMinimal> GetUsers()
        {
            try
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var userService = businessLogic.GetUserService();
                
                // Use business logic UserService instead of direct database access
                var users = userService.GetAllUsersAsync().Result;
                return users.ToList();
            }
            catch (Exception ex)
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var errorHandler = businessLogic.GetErrorHandler();
                errorHandler.LogError(ex, "Error getting users list");
                return new List<UserMinimal>();
            }
        }



        #endregion
    }
} 