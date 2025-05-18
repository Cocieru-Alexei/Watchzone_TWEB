using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WatchZone.Domain.Model;
using WatchZone.Web.Models;

namespace WatchZone.Web.Controllers
{
    public class ListingsController : Controller
    {
        // In-memory storage for demo purposes
        private static List<Listing> _listings = new List<Listing>();
        private static int _nextId = 1;

        // GET: Listings
        public ActionResult Index()
        {
            // Add a demo listing if none exist
            if (_listings.Count == 0)
            {
                _listings.Add(new Listing
                {
                    Id = _nextId++,
                    Title = "Rolex Submariner",
                    Description = "A classic luxury dive watch.",
                    Price = 12000,
                    ImageUrl = "https://www.rolex.com/content/dam/rolex-58/assets/images/watches/submariner/m126610ln-0001/model-page/submariner_m126610ln_0001_2101jva_001.jpg",
                    CreatedAt = DateTime.UtcNow,
                    UserId = 1
                });
            }
            return View(_listings.OrderByDescending(l => l.CreatedAt).ToList());
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
        public ActionResult Create(CreateListingViewModel model)
        {
            if (Request.Cookies["X-KEY"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (ModelState.IsValid)
            {
                var listing = new Listing
                {
                    Id = _nextId++,
                    Title = model.Title,
                    Description = model.Description,
                    Price = model.Price,
                    ImageUrl = model.ImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    UserId = 1 // For demo, static user id
                };

                _listings.Add(listing);

                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Listings/Details/5
        public ActionResult Details(int id)
        {
            var listing = _listings.FirstOrDefault(l => l.Id == id);
            if (listing == null)
            {
                return HttpNotFound();
            }
            return View(listing);
        }

        // GET: Listings/Edit/5
        public ActionResult Edit(int id)
        {
            if (Request.Cookies["X-KEY"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var listing = _listings.FirstOrDefault(l => l.Id == id);
            if (listing == null)
            {
                return HttpNotFound();
            }
            int currentUserId = 1; // TODO: Replace with actual user ID from session
            if (listing.UserId != currentUserId)
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
        public ActionResult Edit(int id, CreateListingViewModel model)
        {
            if (Request.Cookies["X-KEY"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var listing = _listings.FirstOrDefault(l => l.Id == id);
            if (listing == null)
            {
                return HttpNotFound();
            }
            int currentUserId = 1; // TODO: Replace with actual user ID from session
            if (listing.UserId != currentUserId)
            {
                return new HttpStatusCodeResult(403); // Forbidden
            }
            if (ModelState.IsValid)
            {
                listing.Title = model.Title;
                listing.Description = model.Description;
                listing.Price = model.Price;
                listing.ImageUrl = model.ImageUrl;
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
} 