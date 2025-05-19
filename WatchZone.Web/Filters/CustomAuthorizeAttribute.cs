using System.Web;
using System.Web.Mvc;
using WatchZone.BusinessLogic;

namespace WatchZone.Web.Filters
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var cookie = httpContext.Request.Cookies["X-KEY"];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
                return false;

            var bl = new BussinesLogic();
            var session = bl.GetSessionBL();
            var user = session.GetUserByCookie(cookie.Value);
            return user != null;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("~/Auth/Login");
        }
    }
} 