using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;

using WatchZone.Web.Models;
using WatchZone.BusinessLogic.Interface.Repositories;
using WatchZone.Web.Models.Auth;
using WatchZone.Domain.Entities.User;
using WatchZone.BusinessLogic;

namespace WatchZone.Controllers
{
    public class AuthController : Controller
    {
		// GET: Auth
		private readonly ISession _session;
		public AuthController()
		{
			var bl = new BussinesLogic();
			_session = bl.GetSessionBL();
		}
		public ActionResult Login()
		{
			return View();
		}
		public ActionResult Register()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult LoginAction(UserDataLogin login)
		{
			if (ModelState.IsValid)
			{
				var data = Mapper.Map<ULoginData>(login);

				data.Credential = login.UserName;
				data.Password = login.Password;
                data.LoginIp = Request.UserHostAddress;
				data.LoginDateTime = DateTime.Now;

				var userLogin = _session.UserLogin(data);
				if (userLogin.Status)
				{
					HttpCookie cookie = _session.GenCookie(login.UserName);
					ControllerContext.HttpContext.Response.Cookies.Add(cookie);

					return RedirectToAction("Index", "Home");
				}
				else
				{
					ModelState.AddModelError("", userLogin.StatusMsg);
					return View();
				}
			}

			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult RegisterAction(UserDataRegister register)
		{
			if (ModelState.IsValid)
			{
				var data = Mapper.Map<URegisterData>(register);

				data.Credential = register.UserName;
				data.Password = register.Password;
                data.RegisterIp = Request.UserHostAddress;
				data.RegisterDateTime = DateTime.Now;

				var userRegister = _session.UserRegister(data);
				if (userRegister.Status)
				{
					HttpCookie cookie = _session.GenCookie(register.UserName);
					ControllerContext.HttpContext.Response.Cookies.Add(cookie);

					return RedirectToAction("Index", "Home");
				}
				else
				{
					ModelState.AddModelError("", userRegister.StatusMsg);
					return View();
				}
			}

			return View();
		}
        [HttpGet]
        public ActionResult Logout()
        {
            if (Request.Cookies["X-KEY"] != null)
            {
                var cookie = new HttpCookie("X-KEY")
                {
                    Expires = DateTime.Now.AddDays(-1),
                    Value = ""
                };
                Response.Cookies.Add(cookie);
            }
            return RedirectToAction("Index", "Home");
        }
	}
}