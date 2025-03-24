using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace WatchZone.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            var cssBundle = new StyleBundle("~/Content/css");
            cssBundle.Include(
                "~/Content/bootstrap.min.css",
                "~/Content/bootstrap-grid.min.css",
                "~/Content/bootstrap-reboot.min.css",
                "~/Content/styles.css"
                );
            bundles.Add(cssBundle);

            var jsBundle = new ScriptBundle("~/bundles/site");
            jsBundle.Include(
                "~/Scripts/scripts.js"
                );
            bundles.Add(jsBundle);
        }
    }
}