using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WatchZone.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SmartWatch()
        {
            return View();
        }

        public ActionResult SportWatch()
        {
            return View();
        }

        public ActionResult LuxuryWatch()
        {
            return View();
        }

        public ActionResult WatchDetail(string id, string type)
        {

            ViewBag.WatchId = id;
            ViewBag.WatchType = type;

            // Depending on the type, set different watch details
            switch (type)
            {
                case "smart":
                    ViewBag.Title = "Apple Watch Series 8";
                    ViewBag.Price = "$399.99";
                    ViewBag.Description = "The Apple Watch Series 8 features advanced health sensors, Always-On Retina display, and crack-resistant crystal. Track your fitness activities, monitor your heart rate, and stay connected with calls and messages directly from your wrist.";
                    ViewBag.Features = new List<string> {
                        "Always-On Retina display",
                        "Advanced health sensors",
                        "ECG app",
                        "Temperature sensing",
                        "Crack-resistant crystal",
                        "Water resistant up to 50 meters",
                        "18-hour battery life"
                    };
                    ViewBag.ImagePath = "~/Images/smart apple watch.jpg";
                    break;

                case "sport":
                    ViewBag.Title = "Casio G-Shock GBD-200";
                    ViewBag.Price = "$149.99";
                    ViewBag.Description = "Shock-resistant G-Shock watch with fitness tracking functions, Bluetooth connectivity, and a 2-year battery life. Perfect for outdoor activities and extreme sports with its rugged design and water resistance.";
                    ViewBag.Features = new List<string> {
                        "Shock-resistant construction",
                        "Fitness tracking functions",
                        "Bluetooth connectivity",
                        "200m water resistance",
                        "2-year battery life",
                        "LED backlight",
                        "Stopwatch and timer functions"
                    };
                    ViewBag.ImagePath = "~/Images/sport watch casio.jpg";
                    break;

                case "luxury":
                    ViewBag.Title = "Cartier Santos de Cartier";
                    ViewBag.Price = "$6,999.99";
                    ViewBag.Description = "Classic square case design with exposed screws, sword-shaped hands, and QuickSwitch strap system. An iconic timepiece that combines elegance with precision engineering for the discerning watch collector.";
                    ViewBag.Features = new List<string> {
                        "Square case design",
                        "Exposed screws aesthetic",
                        "Sword-shaped hands",
                        "QuickSwitch strap system",
                        "Automatic movement",
                        "Sapphire crystal",
                        "100m water resistance"
                    };
                    ViewBag.ImagePath = "~/Images/luxury cartier_santos.jpg";
                    break;

                default:
                    return RedirectToAction("Index");
            }

            return View();
        }
    }
}