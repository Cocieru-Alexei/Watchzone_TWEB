using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using WatchZone.App_Start;
using AutoMapper;
using WatchZone.Domain.Entities.User;
using WatchZone.Web.Models.Auth;

namespace WatchZone.Web
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
           AreaRegistration.RegisterAllAreas();
           RouteConfig.RegisterRoutes(RouteTable.Routes);
           BundleConfig.RegisterBundles(BundleTable.Bundles);

           Mapper.Initialize(cfg =>
           {
               cfg.CreateMap<UDbTable, UserMinimal>();
               cfg.CreateMap<UserDataLogin, ULoginData>();
               cfg.CreateMap<UserDataRegister, URegisterData>();
           });
        }
    }
}