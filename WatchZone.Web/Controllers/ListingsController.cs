using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WatchZone.Domain.Model;
using WatchZone.Web.Models;
using WatchZone.Domain.Database;
using System.Threading.Tasks;
using WatchZone.Helper;
using WatchZone.BusinessLogic.Database;
using WatchZone.Domain.Entities.User;

namespace WatchZone.Web.Controllers
{
    public class ListingsController : Controller
    {
        // GET: Listings
        public async Task<ActionResult> Index()
        {
            // Get current user ID
            int currentUserId = -1;
            if (Request.Cookies["X-KEY"] != null)
            {
                var xKey = Request.Cookies["X-KEY"].Value;
                var username = WatchZone.Helper.CookieUtility.Validate(xKey);
                using (var db = new WatchZone.BusinessLogic.Database.UserContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Username == username || u.Email == username);
                    if (user != null)
                        currentUserId = user.Id;
                }
            }
            ViewBag.CurrentUserId = currentUserId;

            var dbContext = new DatabaseContext();
            var listings = await dbContext.GetAllListingsAsync();
            return View(listings.OrderByDescending(l => l.CreatedAt).ToList());
        }

        // GET: Listings/Create
        public ActionResult Create()
        {
            if (Request.Cookies["X-KEY"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }

        // POST: Listings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateListingViewModel model)
        {
            if (Request.Cookies["X-KEY"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (ModelState.IsValid)
            {
                // 1. Get the username from the cookie
                var xKey = Request.Cookies["X-KEY"].Value;
                var username = CookieUtility.Validate(xKey);

                // 2. Look up the user in the database
                int userId;
                using (var db = new UserContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Username == username || u.Email == username);
                    if (user == null)
                    {
                        // User not found, handle error or redirect to login
                        return RedirectToAction("Login", "Auth");
                    }
                    userId = user.Id;
                }

                // 3. Create the listing with the correct user id
                var listing = new Listing
                {
                    Title = model.Title,
                    Description = model.Description,
                    Price = model.Price,
                    ImageUrl = model.ImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId
                };

                var dbContext = new DatabaseContext();
                await dbContext.CreateListingAsync(listing);

                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Listings/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var db = new DatabaseContext();
            var listing = await db.GetListingByIdAsync(id);
            if (listing == null)
            {
                return HttpNotFound();
            }
            return View(listing);
        }

        // GET: Listings/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            if (Request.Cookies["X-KEY"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var db = new DatabaseContext();
            var listing = await db.GetListingByIdAsync(id);
            if (listing == null)
            {
                return HttpNotFound();
            }

            // Get current user info
            int currentUserId = -1;
            var isAdmin = false;
            if (Request.Cookies["X-KEY"] != null)
            {
                var xKey = Request.Cookies["X-KEY"].Value;
                var username = WatchZone.Helper.CookieUtility.Validate(xKey);
                using (var userDb = new WatchZone.BusinessLogic.Database.UserContext())
                {
                    var user = userDb.Users.FirstOrDefault(u => u.Username == username || u.Email == username);
                    if (user != null)
                    {
                        currentUserId = user.Id;
                        isAdmin = user.Level.ToString().ToLower() == "admin" || (int)user.Level == 1; // Adjust if needed
                    }
                }
            }

            // Allow if owner or admin
            if (listing.UserId != currentUserId && !isAdmin)
            {
                return new HttpStatusCodeResult(403); // Forbidden
            }

            var model = new CreateListingViewModel
            {
                Title = listing.Title,
                Description = listing.Description,
                Price = listing.Price,
                ImageUrl = listing.ImageUrl
            };
            return View(model);
        }

        // POST: Listings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, CreateListingViewModel model)
        {
            if (Request.Cookies["X-KEY"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var db = new DatabaseContext();
            var listing = await db.GetListingByIdAsync(id);
            if (listing == null)
            {
                return HttpNotFound();
            }

            // Get current user info
            int currentUserId = -1;
            var isAdmin = false;
            if (Request.Cookies["X-KEY"] != null)
            {
                var xKey = Request.Cookies["X-KEY"].Value;
                var username = WatchZone.Helper.CookieUtility.Validate(xKey);
                using (var userDb = new WatchZone.BusinessLogic.Database.UserContext())
                {
                    var user = userDb.Users.FirstOrDefault(u => u.Username == username || u.Email == username);
                    if (user != null)
                    {
                        currentUserId = user.Id;
                        isAdmin = user.Level.ToString().ToLower() == "admin" || (int)user.Level == 1; // Adjust if needed
                    }
                }
            }

            // Allow if owner or admin
            if (listing.UserId != currentUserId && !isAdmin)
            {
                return new HttpStatusCodeResult(403); // Forbidden
            }

            if (ModelState.IsValid)
            {
                await db.UpdateListingAsync(
                    id,
                    model.Title,
                    model.Description,
                    model.Price,
                    model.ImageUrl
                );
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: Listings/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            if (Request.Cookies["X-KEY"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var db = new DatabaseContext();
            var listing = await db.GetListingByIdAsync(id);
            if (listing == null)
            {
                return HttpNotFound();
            }

            // Get current user info
            int currentUserId = -1;
            var isAdmin = false;
            if (Request.Cookies["X-KEY"] != null)
            {
                var xKey = Request.Cookies["X-KEY"].Value;
                var username = WatchZone.Helper.CookieUtility.Validate(xKey);
                using (var userDb = new WatchZone.BusinessLogic.Database.UserContext())
                {
                    var user = userDb.Users.FirstOrDefault(u => u.Username == username || u.Email == username);
                    if (user != null)
                    {
                        currentUserId = user.Id;
                        isAdmin = user.Level.ToString().ToLower() == "admin" || (int)user.Level == 1; // Adjust if needed
                    }
                }
            }

            // Allow if owner or admin
            if (listing.UserId != currentUserId && !isAdmin)
            {
                return new HttpStatusCodeResult(403); // Forbidden
            }

            return View(listing);
        }

        // POST: Listings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (Request.Cookies["X-KEY"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var db = new DatabaseContext();
            var listing = await db.GetListingByIdAsync(id);
            if (listing == null)
            {
                return HttpNotFound();
            }

            // Get current user info
            int currentUserId = -1;
            var isAdmin = false;
            if (Request.Cookies["X-KEY"] != null)
            {
                var xKey = Request.Cookies["X-KEY"].Value;
                var username = WatchZone.Helper.CookieUtility.Validate(xKey);
                using (var userDb = new WatchZone.BusinessLogic.Database.UserContext())
                {
                    var user = userDb.Users.FirstOrDefault(u => u.Username == username || u.Email == username);
                    if (user != null)
                    {
                        currentUserId = user.Id;
                        isAdmin = user.Level.ToString().ToLower() == "admin" || (int)user.Level == 1; // Adjust if needed
                    }
                }
            }

            // Allow if owner or admin
            if (listing.UserId != currentUserId && !isAdmin)
            {
                return new HttpStatusCodeResult(403); // Forbidden
            }

            await db.DeleteListingAsync(id);
            return RedirectToAction("Index");
        }
    }
} 