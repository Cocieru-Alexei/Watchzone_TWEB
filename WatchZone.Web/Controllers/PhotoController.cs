using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WatchZone.BusinessLogic;
using WatchZone.Domain.Model;
using WatchZone.Domain.Enums;

namespace WatchZone.Web.Controllers
{
    public class PhotoController : BaseController
    {
        [HttpPost]
        public async Task<JsonResult> UploadPhoto(int listingId, HttpPostedFileBase file)
        {
            try
            {
                // Use BaseController authentication check
                var authResult = RedirectIfNotAuthenticated();
                if (authResult != null)
                    return Json(new { success = false, message = "Authentication required" });

                if (file == null || file.ContentLength == 0)
                {
                    return Json(new { success = false, message = "No file selected" });
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, message = "Only image files are allowed (jpg, jpeg, png, gif)" });
                }

                // Validate file size (max 5MB)
                if (file.ContentLength > 5 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "File size must be less than 5MB" });
                }

                // Check if user can edit this listing
                var currentUser = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var canEdit = await ListingService.UserCanEditListing(listingId, currentUser.Id, currentUser.Level == URole.Admin);
                if (!canEdit)
                {
                    return Json(new { success = false, message = "You don't have permission to edit this listing" });
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

                // Get current photo count for display order
                var existingPhotos = await ListingService.GetPhotosByListingIdAsync(listingId);
                var displayOrder = existingPhotos.Count() + 1;

                // Create photo record
                var photo = new ListingPhoto
                {
                    ListingId = listingId,
                    FileName = fileName,
                    FilePath = $"/Images/{fileName}",
                    IsPrimary = !existingPhotos.Any(), // First photo is primary
                    DisplayOrder = displayOrder
                };

                var success = await ListingService.AddPhotoAsync(photo);
                if (success)
                {
                    ErrorHandler.LogInfo($"User {currentUser.Username} uploaded photo {fileName} for listing {listingId}");
                    return Json(new { 
                        success = true, 
                        photoId = photo.PhotoId,
                        fileName = fileName,
                        filePath = photo.FilePath,
                        isPrimary = photo.IsPrimary
                    });
                }
                else
                {
                    // Delete the uploaded file if database operation failed
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    return Json(new { success = false, message = "Failed to save photo information" });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "Error uploading photo");
                return Json(new { success = false, message = "An error occurred while uploading the photo" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> DeletePhoto(int photoId)
        {
            try
            {
                // Use BaseController authentication check
                var authResult = RedirectIfNotAuthenticated();
                if (authResult != null)
                    return Json(new { success = false, message = "Authentication required" });

                // Get photo details first
                var photo = await ListingService.GetPhotoByIdAsync(photoId);
                
                if (photo == null)
                {
                    return Json(new { success = false, message = "Photo not found" });
                }

                // Check permissions
                var currentUser = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var canEdit = await ListingService.UserCanEditListing(photo.ListingId, currentUser.Id, currentUser.Level == URole.Admin);
                if (!canEdit)
                {
                    return Json(new { success = false, message = "You don't have permission to delete this photo" });
                }

                // Delete from database
                var success = await ListingService.DeletePhotoAsync(photoId);
                if (success)
                {
                    // Delete physical file
                    var filePath = Server.MapPath($"~{photo.FilePath}");
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    ErrorHandler.LogInfo($"User {currentUser.Username} deleted photo {photo.FileName}");
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete photo" });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "Error deleting photo");
                return Json(new { success = false, message = "An error occurred while deleting the photo" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> SetPrimaryPhoto(int listingId, int photoId)
        {
            try
            {
                // Use BaseController authentication check
                var authResult = RedirectIfNotAuthenticated();
                if (authResult != null)
                    return Json(new { success = false, message = "Authentication required" });

                // Check permissions
                var currentUser = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var canEdit = await ListingService.UserCanEditListing(listingId, currentUser.Id, currentUser.Level == URole.Admin);
                if (!canEdit)
                {
                    return Json(new { success = false, message = "You don't have permission to edit this listing" });
                }

                var success = await ListingService.SetPrimaryPhotoAsync(listingId, photoId);
                if (success)
                {
                    ErrorHandler.LogInfo($"User {currentUser.Username} set photo {photoId} as primary for listing {listingId}");
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to set primary photo" });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "Error setting primary photo");
                return Json(new { success = false, message = "An error occurred while setting the primary photo" });
            }
        }
    }
} 