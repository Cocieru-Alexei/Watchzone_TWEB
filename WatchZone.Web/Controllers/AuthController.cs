using System;
using System.Web.Mvc;
using AutoMapper;
using WatchZone.Web.Models.Auth;
using WatchZone.Domain.Entities.User;

namespace WatchZone.Web.Controllers
{
    public class AuthController : BaseController
    {
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
                try
                {
                    // Explicit business logic instantiation to satisfy ARCH001
                    var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                    var authService = businessLogic.GetAuthService();
                    var errorHandler = businessLogic.GetErrorHandler();

                    var data = Mapper.Map<ULoginData>(login);
                    data.Credential = login.UserName;
                    data.Password = login.Password;
                    data.LoginIp = Request.UserHostAddress;
                    data.LoginDateTime = DateTime.Now;

                    var userLogin = authService.UserLogin(data);
                    if (userLogin.Status)
                    {
                        var cookie = authService.GenerateCookie(login.UserName);
                        if (cookie != null)
                        {
                            ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Login successful but session creation failed.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", userLogin.StatusMsg);
                    }
                }
                catch (Exception ex)
                {
                    // Explicit business logic instantiation to satisfy ARCH001
                    var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                    var errorHandler = businessLogic.GetErrorHandler();
                    errorHandler.LogError(ex, "Error during login process");
                    ModelState.AddModelError("", "An error occurred during login. Please try again.");
                }
            }

            return View("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterAction(UserDataRegister register)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Explicit business logic instantiation to satisfy ARCH001
                    var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                    var authService = businessLogic.GetAuthService();
                    var errorHandler = businessLogic.GetErrorHandler();

                    var data = Mapper.Map<URegisterData>(register);
                    data.Credential = register.UserName;
                    data.Password = register.Password;
                    data.RegisterIp = Request.UserHostAddress;
                    data.RegisterDateTime = DateTime.Now;

                    var userRegister = authService.UserRegister(data);
                    if (userRegister.Status)
                    {
                        var cookie = authService.GenerateCookie(register.UserName);
                        if (cookie != null)
                        {
                            ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                            errorHandler.LogInfo($"User {register.UserName} registered successfully from IP {Request.UserHostAddress}");
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            errorHandler.LogWarning($"Registration successful for {register.UserName} but session creation failed");
                            ModelState.AddModelError("", "Registration successful but session creation failed.");
                        }
                    }
                    else
                    {
                        errorHandler.LogWarning($"Failed registration attempt for {register.UserName}: {userRegister.StatusMsg}");
                        ModelState.AddModelError("", userRegister.StatusMsg);
                    }
                }
                catch (Exception ex)
                {
                    // Explicit business logic instantiation to satisfy ARCH001
                    var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                    var errorHandler = businessLogic.GetErrorHandler();
                    errorHandler.LogError(ex, "Error during registration process");
                    ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                }
            }

            return View("Register");
        }

        [HttpGet]
        public ActionResult Logout()
        {
            try
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var authService = businessLogic.GetAuthService();
                var errorHandler = businessLogic.GetErrorHandler();

                // Log the logout attempt
                var currentUser = authService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser != null)
                {
                    errorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) logged out");
                }

                LogoutUser();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var errorHandler = businessLogic.GetErrorHandler();
                errorHandler.LogError(ex, "Error during logout process");
                return RedirectToAction("Index", "Home");
            }
        }
    }
}