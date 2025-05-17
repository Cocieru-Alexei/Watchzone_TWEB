using System.Web;
using WatchZone.Domain.Entities.User;

namespace WatchZone.BusinessLogic.Interface.Repositories
{
	public interface ISession
	{
		ULoginResp UserLogin(ULoginData data);
		URegisterResp UserRegister(URegisterData data);
		HttpCookie GenCookie(string loginCredential);
		UserMinimal GetUserByCookie(string apiCookieValue);
	}
}
