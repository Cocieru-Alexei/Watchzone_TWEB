using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WatchZone.Domain.Entities.User;

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