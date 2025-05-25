using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WatchZone.Web.Models.Profile;
using WatchZone.Helper;

namespace WatchZone.Web.Controllers
{
    public class ProfileController : BaseController
    {
        // GET: Profile
        public async Task<ActionResult> Index()
        {
            var authResult = RedirectIfNotAuthenticated();
            if (authResult != null)
                return authResult;

            try
            {
                var currentUser = GetCurrentUser();
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Get user listings
                var userListings = await ListingService.GetListingsByUserIdAsync(currentUser.Id);
                
                // Load photos for each listing
                foreach (var listing in userListings)
                {
                    listing.Photos = (await ListingService.GetPhotosByListingIdAsync(listing.Listings_Id)).ToList();
                }

                var model = new ProfileViewModel
                {
                    User = currentUser,
                    UserListings = userListings.OrderByDescending(l => l.CreatedAt).ToList()
                };

                ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) accessed profile page");
                return View(model);
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "Error loading profile page");
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Profile/ChangePassword
        public ActionResult ChangePassword()
        {
            var authResult = RedirectIfNotAuthenticated();
            if (authResult != null)
                return authResult;

            return View();
        }

        // POST: Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var authResult = RedirectIfNotAuthenticated();
            if (authResult != null)
                return authResult;

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = GetCurrentUser();
                    if (currentUser == null)
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    // Verify current password
                    if (!UserService.VerifyPassword(currentUser.Id, model.CurrentPassword))
                    {
                        ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                        return View(model);
                    }

                    // Update password
                    var success = UserService.UpdatePassword(currentUser.Id, model.NewPassword);
                    
                    if (success)
                    {
                        ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) changed password successfully");
                        TempData["SuccessMessage"] = "Password changed successfully!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ErrorHandler.LogWarning($"Failed to change password for user {currentUser.Username} (ID: {currentUser.Id})");
                        ModelState.AddModelError("", "Failed to change password. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogError(ex, "Error changing password");
                    ModelState.AddModelError("", "An error occurred while changing password. Please try again.");
                }
            }

            return View(model);
        }
    }
} 