using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchZone.Domain.Model.User;
using System.Web;
using WatchZone.Domain.Entities.User;
using WatchZone.Domain.Enums;

namespace WatchZone.BusinessLogic.Interface
{
    public interface IAuth
    {
        string UserAuthLogic(UserLoginDTO data);
        ULoginResp UserLogin(ULoginData data);
        URegisterResp UserRegister(URegisterData data);
        HttpCookie GenerateCookie(string loginCredential);
        UserMinimal GetUserByCookie(string cookieValue);
        URole? GetUserRoleByCookie(string cookieValue);
        void ExpireUserSession(string cookieValue);
        bool ValidateUserAccess(string cookieValue, URole requiredRole = URole.User);
        int? GetCurrentUserId(string cookieValue);
    }
} 