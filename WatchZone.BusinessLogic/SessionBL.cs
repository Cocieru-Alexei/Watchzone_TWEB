using System.Web;
using WatchZone.BusinessLogic.Core;
using WatchZone.BusinessLogic.Interface.Repositories;
using WatchZone.Domain.Entities.User;

namespace WatchZone.BusinessLogic
{
    public class SessionBL : UserApi, ISession
    {
        public ULoginResp UserLogin(ULoginData data)
        {
            return UserLoginAction(data);
        }

        public HttpCookie GenCookie(string loginCredential)
        {
            return Cookie(loginCredential);
        }

        public UserMinimal GetUserByCookie(string apiCookieValue)
        {
            return UserCookie(apiCookieValue);
        }

		public URegisterResp UserRegister(URegisterData data)
		{
            return UserRegisterAction(data);
		}
	}
}
