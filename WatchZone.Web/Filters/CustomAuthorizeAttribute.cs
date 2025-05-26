using System.Web;
using System.Web.Mvc;
using WatchZone.BusinessLogic;
using WatchZone.Helper;

namespace WatchZone.Web.Filters
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var cookie = httpContext.Request.Cookies["X-KEY"];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
                return false;

            // Use static utility method to avoid instantiating business logic in attribute
            // This is a compromise for attributes which can't use dependency injection
            try
            {
                var username = CookieUtility.Validate(cookie.Value);
                return !string.IsNullOrEmpty(username);
            }
            catch
            {
                return false;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("~/Auth/Login");
        }
    }
} 