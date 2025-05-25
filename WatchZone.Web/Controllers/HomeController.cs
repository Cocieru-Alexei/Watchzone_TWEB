using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WatchZone.BusinessLogic;
using WatchZone.Domain.Entities.User;
using WatchZone.Domain.Enums;
using WatchZone.Domain.Model;

namespace WatchZone.Web.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Home
        public async Task<ActionResult> Index()
        {
            try
            {
                // Use business logic to log user visit and get user context
                var currentUser = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser != null)
                {
                    ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) visited home page");
                    ViewBag.IsAuthenticated = true;
                    ViewBag.UserName = currentUser.Username;
                    ViewBag.UserRole = currentUser.Level.ToString();
                }
                else
                {
                    ErrorHandler.LogInfo("Anonymous user visited home page");
                    ViewBag.IsAuthenticated = false;
                }

                // Use business logic to get featured listings for the home page (only available ones)
                var availableListings = await ListingService.GetAvailableListingsAsync();
                var featuredListings = availableListings.Take(3).ToList();
                
                // Load photos for each featured listing
                foreach (var listing in featuredListings)
                {
                    listing.Photos = (await ListingService.GetPhotosByListingIdAsync(listing.Listings_Id)).ToList();
                }
                
                ViewBag.FeaturedListings = featuredListings;
                
                return View();
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Unable to load home page");
            }
        }

        public async Task<ActionResult> SmartWatch()
        {
            try
            {
                // Use business logic to log user visit and get user context
                var currentUser = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser != null)
                {
                    ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) visited Smart Watch category");
                }
                else
                {
                    ErrorHandler.LogInfo("Anonymous user visited Smart Watch category");
                }

                // Use business logic to get smart watch listings
                var allListings = await ListingService.GetAllListingsAsync();
                var smartWatchListings = allListings
                    .Where(l => l.Title.ToLower().Contains("smart") || l.Description.ToLower().Contains("smart"))
                    .ToList();
                ViewBag.CategoryListings = smartWatchListings;
                ViewBag.CategoryName = "Smart Watches";
                
                return View();
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Unable to load Smart Watch category");
            }
        }

        public async Task<ActionResult> SportWatch()
        {
            try
            {
                // Use business logic to log user visit and get user context
                var currentUser = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser != null)
                {
                    ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) visited Sport Watch category");
                }
                else
                {
                    ErrorHandler.LogInfo("Anonymous user visited Sport Watch category");
                }

                // Use business logic to get sport watch listings
                var allListings = await ListingService.GetAllListingsAsync();
                var sportWatchListings = allListings
                    .Where(l => l.Title.ToLower().Contains("sport") || l.Description.ToLower().Contains("sport") || 
                               l.Title.ToLower().Contains("casio") || l.Title.ToLower().Contains("g-shock"))
                    .ToList();
                ViewBag.CategoryListings = sportWatchListings;
                ViewBag.CategoryName = "Sport Watches";
                
                return View();
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Unable to load Sport Watch category");
            }
        }

        public async Task<ActionResult> LuxuryWatch()
        {
            try
            {
                // Use business logic to log user visit and get user context
                var currentUser = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser != null)
                {
                    ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) visited Luxury Watch category");
                }
                else
                {
                    ErrorHandler.LogInfo("Anonymous user visited Luxury Watch category");
                }

                // Use business logic to get luxury watch listings
                var allListings = await ListingService.GetAllListingsAsync();
                var luxuryWatchListings = allListings
                    .Where(l => l.Title.ToLower().Contains("luxury") || l.Description.ToLower().Contains("luxury") ||
                               l.Title.ToLower().Contains("cartier") || l.Price > 1000)
                    .ToList();
                ViewBag.CategoryListings = luxuryWatchListings;
                ViewBag.CategoryName = "Luxury Watches";
                
                return View();
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Unable to load Luxury Watch category");
            }
        }

        public async Task<ActionResult> AdminPanel()
        {
            // Use the centralized authentication/authorization from BaseController
            var accessResult = RedirectIfNotAdmin();
            if (accessResult != null)
            {
                return accessResult;
            }

            try
            {
                // Use the UserService from dependency injection
                var users = await UserService.GetAllUsersAsync();
                ViewBag.Users = users.ToList();
                
                return View();
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Unable to load admin panel");
            }
        }

        public async Task<ActionResult> WatchDetail(string id, string type)
        {
            try
            {
                // Use business logic to validate input parameters
                if (string.IsNullOrEmpty(id))
                {
                    ErrorHandler.LogWarning("WatchDetail accessed with null or empty id parameter");
                    return RedirectToAction("Index");
                }

                if (!int.TryParse(id, out int watchId))
                {
                    ErrorHandler.LogWarning($"WatchDetail accessed with invalid id parameter: {id}");
                    return RedirectToAction("Index");
                }

                // Use business logic to log user visit
                var currentUser = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser != null)
                {
                    ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) viewed watch detail: ID {watchId}, Type {type}");
                }
                else
                {
                    ErrorHandler.LogInfo($"Anonymous user viewed watch detail: ID {watchId}, Type {type}");
                }

                // Use business logic to get actual listing data
                var listing = await ListingService.GetListingByIdAsync(watchId);
                if (listing == null)
                {
                    ErrorHandler.LogWarning($"Watch detail requested for non-existent listing: ID {watchId}");
                    TempData["ErrorMessage"] = "The requested watch could not be found.";
                    return RedirectToAction("Index");
                }

                // Use business logic to validate type parameter against actual data
                if (!string.IsNullOrEmpty(type))
                {
                    var expectedType = GetWatchTypeFromTitle(listing.Title);
                    if (!string.Equals(type, expectedType, StringComparison.OrdinalIgnoreCase))
                    {
                        ErrorHandler.LogWarning($"Watch detail accessed with mismatched type. Expected: {expectedType}, Provided: {type}, Listing ID: {watchId}");
                    }
                }

                // Set ViewBag data from business logic
                ViewBag.WatchId = listing.Listings_Id;
                ViewBag.WatchType = type;
                ViewBag.Listing = listing;

                // Use business logic to log successful access
                ErrorHandler.LogInfo($"Successfully loaded watch detail: {listing.Title} (ID: {watchId}) - Price: {listing.Price:C}");

                return View(listing);
            }
            catch (Exception ex)
            {
                return HandleError(ex, $"Unable to load watch detail for ID: {id}");
            }
        }

        public ActionResult FeaturedWatches()
        {
            ViewBag.CategoryName = "Featured Watches";
            return View();
        }

        public ActionResult SmartWatchDetail()
        {
            return View();
        }

        public ActionResult SportWatchDetail()
        {
            return View();
        }

        public ActionResult LuxuryWatchDetail()
        {
            return View();
        }

        public ActionResult SeikoSKX007Detail()
        {
            return View();
        }

        public ActionResult SpeedmasterDetail()
        {
            return View();
        }

        public ActionResult TissotPRXDetail()
        {
            return View();
        }

        // Helper method using business logic principles
        private string GetWatchTypeFromTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                return "unknown";

            title = title.ToLower();
            if (title.Contains("smart") || title.Contains("apple"))
                return "smart";
            if (title.Contains("sport") || title.Contains("casio") || title.Contains("g-shock"))
                return "sport";
            if (title.Contains("luxury") || title.Contains("cartier"))
                return "luxury";

            return "general";
        }
    }
}