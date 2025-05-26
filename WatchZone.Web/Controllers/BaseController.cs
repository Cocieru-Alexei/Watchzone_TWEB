using System.Web;
using System.Web.Mvc;
using WatchZone.BusinessLogic;
using WatchZone.BusinessLogic.Interface;
using WatchZone.BusinessLogic.Services;
using WatchZone.Domain.Entities.User;
using WatchZone.Domain.Enums;

namespace WatchZone.Web.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IAuth AuthService;
        protected readonly IListingService ListingService;
        protected readonly IUserService UserService;
        protected readonly ICartService CartService;
        protected readonly IErrorHandler ErrorHandler;
        protected readonly OrderService OrderService;
        protected readonly IReviewService ReviewService;

        public BaseController()
        {
            // Use ServiceLocator pattern for centralized service resolution
            // This is better than manual instantiation and maintains compatibility
            // with legacy .NET Framework MVC while being more testable
            AuthService = ServiceLocator.GetAuthService();
            ListingService = ServiceLocator.GetListingService();
            UserService = ServiceLocator.GetUserService();
            CartService = ServiceLocator.GetCartService();
            ErrorHandler = ServiceLocator.GetErrorHandler();
            OrderService = ServiceLocator.GetOrderService();
            ReviewService = ServiceLocator.GetReviewService();
        }

        /// <summary>
        /// Gets the current user from the X-KEY cookie
        /// </summary>
        protected UserMinimal GetCurrentUser()
        {
            var cookie = Request.Cookies["X-KEY"];
            if (cookie != null)
            {
                return AuthService.GetUserByCookie(cookie.Value);
            }
            return null;
        }

        /// <summary>
        /// Gets the current user's ID from the X-KEY cookie
        /// </summary>
        protected int? GetCurrentUserId()
        {
            var cookie = Request.Cookies["X-KEY"];
            if (cookie != null)
            {
                return AuthService.GetCurrentUserId(cookie.Value);
            }
            return null;
        }

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        protected bool IsAuthenticated()
        {
            var cookie = Request.Cookies["X-KEY"];
            return cookie != null && AuthService.GetUserByCookie(cookie.Value) != null;
        }

        /// <summary>
        /// Checks if the current user has the required role
        /// </summary>
        protected bool HasRole(URole requiredRole)
        {
            var cookie = Request.Cookies["X-KEY"];
            if (cookie != null)
            {
                return AuthService.ValidateUserAccess(cookie.Value, requiredRole);
            }
            return false;
        }

        /// <summary>
        /// Checks if the current user is an admin
        /// </summary>
        protected bool IsAdmin()
        {
            return HasRole(URole.Admin);
        }

        /// <summary>
        /// Redirects to login if the user is not authenticated
        /// </summary>
        protected ActionResult RedirectIfNotAuthenticated()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }
            return null;
        }

        /// <summary>
        /// Redirects to access denied if the user doesn't have the required role
        /// </summary>
        protected ActionResult RedirectIfNoAccess(URole requiredRole)
        {
            var authCheck = RedirectIfNotAuthenticated();
            if (authCheck != null)
                return authCheck;

            if (!HasRole(requiredRole))
            {
                return Content("<div style='text-align: center; margin-top: 50px;'>" +
                              "<h1>Access Denied</h1>" +
                              "<p>You don't have permission to access this page.</p>" +
                              "<a href='/Home/Index' class='btn btn-primary'>Go to Home</a>" +
                              "</div>");
            }
            return null;
        }

        /// <summary>
        /// Redirects to access denied if the user is not an admin
        /// </summary>
        protected ActionResult RedirectIfNotAdmin()
        {
            return RedirectIfNoAccess(URole.Admin);
        }



        /// <summary>
        /// Handles errors in a consistent way across controllers
        /// </summary>
        protected ActionResult HandleError(System.Exception exception, string defaultMessage = "An error occurred")
        {
            if (ErrorHandler.HandleError(exception, out string userMessage))
            {
                TempData["ErrorMessage"] = userMessage;
            }
            else
            {
                TempData["ErrorMessage"] = defaultMessage;
            }
            
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Logs out the current user by expiring their session and cookie
        /// </summary>
        protected void LogoutUser()
        {
            var cookie = Request.Cookies["X-KEY"];
            if (cookie != null)
            {
                AuthService.ExpireUserSession(cookie.Value);
                
                // Expire the cookie on the client side
                var expiredCookie = new HttpCookie("X-KEY")
                {
                    Expires = System.DateTime.Now.AddDays(-1),
                    Value = ""
                };
                Response.Cookies.Add(expiredCookie);
            }
        }
    }
} 