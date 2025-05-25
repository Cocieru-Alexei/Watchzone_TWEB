using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WatchZone.BusinessLogic;
using WatchZone.BusinessLogic.Interface;
using WatchZone.Domain.Model;
using WatchZone.Web.Models;

namespace WatchZone.Web.Controllers
{
    public class ListingsController : BaseController
    {
        // GET: Listings
        public async Task<ActionResult> Index()
        {
            try
            {
                // Get current user ID using business logic
                var currentUser = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                var currentUserId = currentUser?.Id ?? -1;
                ViewBag.CurrentUserId = currentUserId;

                // Log user access
                if (currentUser != null)
                {
                    ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) accessed listings page");
                }
                else
                {
                    ErrorHandler.LogInfo("Anonymous user accessed listings page");
                }

                // Use the business logic ListingService to get only available (unsold) listings
                var listings = await ListingService.GetAvailableListingsAsync();
                
                // Load photos for each listing
                foreach (var listing in listings)
                {
                    listing.Photos = (await ListingService.GetPhotosByListingIdAsync(listing.Listings_Id)).ToList();
                }
                
                return View(listings.OrderByDescending(l => l.CreatedAt).ToList());
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "Unable to load listings");
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Listings/Create
        public ActionResult Create()
        {
            // Use BaseController authentication check
            var authResult = RedirectIfNotAuthenticated();
            if (authResult != null)
                return authResult;

            try
            {
                // Log user access to create form
                var currentUser = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser != null)
                {
                    ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) accessed create listing form");
                }

                return View();
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "Error loading create listing form");
                return RedirectToAction("Index");
            }
        }

        // POST: Listings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateListingViewModel model)
        {
            // Use BaseController authentication check
            var authResult = RedirectIfNotAuthenticated();
            if (authResult != null)
                return authResult;

            if (ModelState.IsValid)
            {
                try
                {
                    // Get current user ID using business logic
                    var currentUser = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                    if (currentUser == null)
                    {
                        ErrorHandler.LogWarning("Create listing attempted by unauthenticated user");
                        return RedirectToAction("Login", "Auth");
                    }

                    // Create the listing with the correct user id
                    var listing = new Listing
                    {
                        Title = model.Title,
                        Description = model.Description,
                        Price = model.Price,
                        ImageUrl = model.ImageUrl,
                        CreatedAt = DateTime.UtcNow,
                        UserId = currentUser.Id
                    };

                    // Use the business logic ListingService
                    var success = await ListingService.CreateListingAsync(listing);
                    if (success)
                    {
                        ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) created listing: {listing.Title}");
                        
                        // Handle photo uploads if any
                        if (model.Photos != null && model.Photos.Any(p => p != null))
                        {
                            await HandlePhotoUploads(model.Photos, listing.Listings_Id, ErrorHandler);
                        }
                        
                        return RedirectToAction("Details", new { id = listing.Listings_Id });
                    }
                    else
                    {
                        ErrorHandler.LogWarning($"Failed to create listing for user {currentUser.Username}: {listing.Title}");
                        ModelState.AddModelError("", "Failed to create listing. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogError(ex, "Error creating listing");
                    ModelState.AddModelError("", "An error occurred while creating the listing.");
                }
            }

            return View(model);
        }

        // GET: Listings/Details/5
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                // Log user access
                var currentUser = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                var currentUserId = currentUser?.Id ?? -1;
                ViewBag.CurrentUserId = currentUserId;
                
                if (currentUser != null)
                {
                    ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) viewed listing details: ID {id}");
                }
                else
                {
                    ErrorHandler.LogInfo($"Anonymous user viewed listing details: ID {id}");
                }

                var listing = await ListingService.GetListingByIdAsync(id);
                if (listing == null)
                {
                    ErrorHandler.LogWarning($"Listing details requested for non-existent listing: ID {id}");
                    return HttpNotFound();
                }

                // Load photos for the listing
                listing.Photos = (await ListingService.GetPhotosByListingIdAsync(id)).ToList();
                
                // Check if the listing is sold
                var isSold = await ListingService.IsListingSoldAsync(id);
                ViewBag.IsSold = isSold;
                
                return View(listing);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "Unable to load listing details");
                return RedirectToAction("Index");
            }
        }

        // GET: Listings/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            // Use BaseController authentication check
            var authResult = RedirectIfNotAuthenticated();
            if (authResult != null)
                return authResult;

            try
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new BussinesLogic();
                var listingService = businessLogic.GetListingService();
                var authService = businessLogic.GetAuthService();
                var errorHandler = businessLogic.GetErrorHandler();

                var listing = await listingService.GetListingByIdAsync(id);
                if (listing == null)
                {
                    errorHandler.LogWarning($"Edit requested for non-existent listing: ID {id}");
                    return HttpNotFound();
                }

                // Use business logic to check edit permissions
                var currentUser = authService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser == null || !CanEditListing(id))
                {
                    errorHandler.LogWarning($"Unauthorized edit attempt for listing {id} by user {currentUser?.Username ?? "anonymous"}");
                    return new HttpStatusCodeResult(403); // Forbidden
                }

                errorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) accessed edit form for listing: {listing.Title} (ID: {id})");

                // Load existing photos
                var existingPhotos = await listingService.GetPhotosByListingIdAsync(id);
                ViewBag.ExistingPhotos = existingPhotos.ToList();
                ViewBag.ListingId = id;

                var model = new CreateListingViewModel
                {
                    Title = listing.Title,
                    Description = listing.Description,
                    Price = listing.Price,
                    ImageUrl = listing.ImageUrl
                };
                return View(model);
            }
            catch (Exception ex)
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new BussinesLogic();
                var errorHandler = businessLogic.GetErrorHandler();
                errorHandler.LogError(ex, "Unable to load listing for editing");
                return RedirectToAction("Index");
            }
        }

        // POST: Listings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, CreateListingViewModel model)
        {
            // Use BaseController authentication check
            var authResult = RedirectIfNotAuthenticated();
            if (authResult != null)
                return authResult;

            try
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new BussinesLogic();
                var listingService = businessLogic.GetListingService();
                var authService = businessLogic.GetAuthService();
                var errorHandler = businessLogic.GetErrorHandler();

                var listing = await listingService.GetListingByIdAsync(id);
                if (listing == null)
                {
                    errorHandler.LogWarning($"Edit update attempted for non-existent listing: ID {id}");
                    return HttpNotFound();
                }

                // Use business logic to check edit permissions
                var currentUser = authService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser == null || !CanEditListing(id))
                {
                    errorHandler.LogWarning($"Unauthorized edit update attempt for listing {id} by user {currentUser?.Username ?? "anonymous"}");
                    return new HttpStatusCodeResult(403); // Forbidden
                }

                if (ModelState.IsValid)
                {
                    // Update listing properties
                    listing.Title = model.Title;
                    listing.Description = model.Description;
                    listing.Price = model.Price;
                    listing.ImageUrl = model.ImageUrl;

                    var success = await listingService.UpdateListingAsync(listing);
                    if (success)
                    {
                        errorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) updated listing: {listing.Title} (ID: {id})");
                        
                        // Handle photo uploads if any
                        if (model.Photos != null && model.Photos.Any(p => p != null))
                        {
                            await HandlePhotoUploads(model.Photos, id, errorHandler);
                        }
                        
                        return RedirectToAction("Details", new { id = id });
                    }
                    else
                    {
                        errorHandler.LogWarning($"Failed to update listing {id} for user {currentUser.Username}");
                        ModelState.AddModelError("", "Failed to update listing. Please try again.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new BussinesLogic();
                var errorHandler = businessLogic.GetErrorHandler();
                errorHandler.LogError(ex, "Error updating listing");
                ModelState.AddModelError("", "An error occurred while updating the listing.");
            }

            // Load existing photos for the view in case of validation errors
            try
            {
                var businessLogic = new BussinesLogic();
                var listingService = businessLogic.GetListingService();
                var existingPhotos = await listingService.GetPhotosByListingIdAsync(id);
                ViewBag.ExistingPhotos = existingPhotos.ToList();
                ViewBag.ListingId = id;
            }
            catch
            {
                ViewBag.ExistingPhotos = new List<ListingPhoto>();
                ViewBag.ListingId = id;
            }

            return View(model);
        }

        /// <summary>
        /// Helper method to handle photo uploads for listings
        /// </summary>
        private async Task HandlePhotoUploads(List<HttpPostedFileBase> photos, int listingId, IErrorHandler errorHandler)
        {
            try
            {
                
                // Get existing photos to determine display order
                var existingPhotos = await ListingService.GetPhotosByListingIdAsync(listingId);
                var nextDisplayOrder = existingPhotos.Any() ? existingPhotos.Max(p => p.DisplayOrder) + 1 : 1;
                var isFirstPhoto = !existingPhotos.Any();

                foreach (var file in photos.Where(p => p != null && p.ContentLength > 0))
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        errorHandler.LogWarning($"Invalid file type uploaded: {file.FileName}");
                        continue;
                    }

                    // Validate file size (max 5MB)
                    if (file.ContentLength > 5 * 1024 * 1024)
                    {
                        errorHandler.LogWarning($"File too large uploaded: {file.FileName}");
                        continue;
                    }

                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var uploadPath = Server.MapPath("~/Images/");
                    var filePath = Path.Combine(uploadPath, fileName);

                    // Ensure directory exists
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    // Save file
                    file.SaveAs(filePath);

                    // Create photo record
                    var photo = new ListingPhoto
                    {
                        ListingId = listingId,
                        FileName = fileName,
                        FilePath = $"/Images/{fileName}",
                        IsPrimary = isFirstPhoto, // First photo is primary
                        DisplayOrder = nextDisplayOrder
                    };

                    var success = await ListingService.AddPhotoAsync(photo);
                    if (success)
                    {
                        errorHandler.LogInfo($"Photo uploaded successfully: {fileName} for listing {listingId}");
                        nextDisplayOrder++;
                        isFirstPhoto = false; // Only the first photo should be primary
                    }
                    else
                    {
                        // Delete the uploaded file if database operation failed
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                        errorHandler.LogError(null, $"Failed to save photo information for {fileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                errorHandler.LogError(ex, "Error handling photo uploads");
            }
        }

        // GET: Listings/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            // Use BaseController authentication check
            var authResult = RedirectIfNotAuthenticated();
            if (authResult != null)
                return authResult;

            try
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new BussinesLogic();
                var listingService = businessLogic.GetListingService();
                var authService = businessLogic.GetAuthService();
                var errorHandler = businessLogic.GetErrorHandler();

                var listing = await listingService.GetListingByIdAsync(id);
                if (listing == null)
                {
                    errorHandler.LogWarning($"Delete requested for non-existent listing: ID {id}");
                    return HttpNotFound();
                }

                // Use business logic to check edit permissions
                var currentUser = authService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser == null || !CanEditListing(id))
                {
                    errorHandler.LogWarning($"Unauthorized delete attempt for listing {id} by user {currentUser?.Username ?? "anonymous"}");
                    return new HttpStatusCodeResult(403); // Forbidden
                }

                errorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) accessed delete confirmation for listing: {listing.Title} (ID: {id})");
                return View(listing);
            }
            catch (Exception ex)
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new BussinesLogic();
                var errorHandler = businessLogic.GetErrorHandler();
                errorHandler.LogError(ex, "Unable to load listing for deletion");
                return RedirectToAction("Index");
            }
        }

        // POST: Listings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            // Use BaseController authentication check
            var authResult = RedirectIfNotAuthenticated();
            if (authResult != null)
                return authResult;

            try
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new BussinesLogic();
                var listingService = businessLogic.GetListingService();
                var authService = businessLogic.GetAuthService();
                var errorHandler = businessLogic.GetErrorHandler();

                // Use business logic to check edit permissions
                var currentUser = authService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser == null || !CanEditListing(id))
                {
                    errorHandler.LogWarning($"Unauthorized delete confirmation for listing {id} by user {currentUser?.Username ?? "anonymous"}");
                    return new HttpStatusCodeResult(403); // Forbidden
                }

                // Get listing details for logging before deletion
                var listing = await listingService.GetListingByIdAsync(id);
                var listingTitle = listing?.Title ?? $"Unknown (ID: {id})";

                var success = await listingService.DeleteListingAsync(id);
                if (success)
                {
                    errorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) deleted listing: {listingTitle} (ID: {id})");
                    return RedirectToAction("Index");
                }
                else
                {
                    errorHandler.LogWarning($"Failed to delete listing {id} ({listingTitle}) for user {currentUser.Username}");
                    TempData["ErrorMessage"] = "Failed to delete listing. Please try again.";
                    return RedirectToAction("Delete", new { id = id });
                }
            }
            catch (Exception ex)
            {
                // Explicit business logic instantiation to satisfy ARCH001
                var businessLogic = new BussinesLogic();
                var errorHandler = businessLogic.GetErrorHandler();
                errorHandler.LogError(ex, "Error deleting listing");
                return RedirectToAction("Index");
            }
        }
    }
} 