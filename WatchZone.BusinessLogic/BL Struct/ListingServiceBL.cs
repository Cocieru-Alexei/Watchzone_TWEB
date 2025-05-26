using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WatchZone.BusinessLogic.Interface;
using WatchZone.Domain.Database;
using WatchZone.Domain.Model;

namespace WatchZone.BusinessLogic.BL_Struct
{
    public class ListingServiceBL : IListingService
    {
        private readonly IErrorHandler _errorHandler;

        public ListingServiceBL(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public async Task<IEnumerable<Listing>> GetAllListingsAsync()
        {
            try
            {
                var context = new DatabaseContext();
                return await context.GetAllListingsAsync();
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "Error getting all listings");
                return Enumerable.Empty<Listing>();
            }
        }

        public async Task<IEnumerable<Listing>> GetAvailableListingsAsync()
        {
            try
            {
                var context = new DatabaseContext();
                return await context.GetAvailableListingsAsync();
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "Error getting available listings");
                return Enumerable.Empty<Listing>();
            }
        }

        public async Task<IEnumerable<Listing>> SearchListingsAsync(string searchQuery)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchQuery))
                {
                    // If no search query, return available listings
                    return await GetAvailableListingsAsync();
                }

                var context = new DatabaseContext();
                return await context.SearchListingsAsync(searchQuery);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error searching listings with query: {searchQuery}");
                return Enumerable.Empty<Listing>();
            }
        }

        public async Task<bool> IsListingSoldAsync(int listingId)
        {
            try
            {
                var context = new DatabaseContext();
                return await context.IsListingSoldAsync(listingId);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error checking if listing {listingId} is sold");
                return false;
            }
        }

        public async Task<Listing> GetListingByIdAsync(int id)
        {
            try
            {
                var context = new DatabaseContext();
                return await context.GetListingByIdAsync(id);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error getting listing with ID: {id}");
                return null;
            }
        }

        public async Task<bool> CreateListingAsync(Listing listing)
        {
            try
            {
                if (listing == null)
                {
                    _errorHandler.LogWarning("Attempted to create null listing");
                    return false;
                }

                listing.CreatedAt = DateTime.UtcNow;
                
                var context = new DatabaseContext();
                var newId = await context.CreateListingAsync(listing);
                listing.Listings_Id = newId;
                _errorHandler.LogInfo($"Created new listing: {listing.Title} by user {listing.UserId}");
                return true;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error creating listing: {listing?.Title}");
                return false;
            }
        }

        public async Task<bool> UpdateListingAsync(Listing listing)
        {
            try
            {
                if (listing == null)
                {
                    _errorHandler.LogWarning("Attempted to update null listing");
                    return false;
                }

                var context = new DatabaseContext();
                await context.UpdateListingAsync(listing.Listings_Id, listing.Title, listing.Description, listing.Price, listing.ImageUrl);
                _errorHandler.LogInfo($"Updated listing: {listing.Title} (ID: {listing.Listings_Id})");
                return true;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error updating listing: {listing?.Title} (ID: {listing?.Listings_Id})");
                return false;
            }
        }

        public async Task<bool> DeleteListingAsync(int id)
        {
            try
            {
                var context = new DatabaseContext();
                await context.DeleteListingAsync(id);
                _errorHandler.LogInfo($"Deleted listing with ID: {id}");
                return true;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error deleting listing with ID: {id}");
                return false;
            }
        }

        public async Task<IEnumerable<Listing>> GetListingsByUserIdAsync(int userId)
        {
            try
            {
                var context = new DatabaseContext();
                var allListings = await context.GetAllListingsAsync();
                return allListings.Where(l => l.UserId == userId);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error getting listings for user ID: {userId}");
                return Enumerable.Empty<Listing>();
            }
        }

        public async Task<bool> UserCanEditListing(int listingId, int userId, bool isAdmin)
        {
            try
            {
                if (isAdmin)
                    return true;

                var listing = await GetListingByIdAsync(listingId);
                return listing != null && listing.UserId == userId;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error checking edit permissions for listing {listingId} and user {userId}");
                return false;
            }
        }

        // Photo management methods
        public async Task<IEnumerable<ListingPhoto>> GetPhotosByListingIdAsync(int listingId)
        {
            try
            {
                var context = new DatabaseContext();
                return await context.GetPhotosByListingIdAsync(listingId);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error getting photos for listing ID: {listingId}");
                return Enumerable.Empty<ListingPhoto>();
            }
        }

        public async Task<ListingPhoto> GetPhotoByIdAsync(int photoId)
        {
            try
            {
                var context = new DatabaseContext();
                return await context.GetPhotoByIdAsync(photoId);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error getting photo with ID: {photoId}");
                return null;
            }
        }

        public async Task<bool> AddPhotoAsync(ListingPhoto photo)
        {
            try
            {
                if (photo == null)
                {
                    _errorHandler.LogWarning("Attempted to add null photo");
                    return false;
                }

                photo.UploadedAt = DateTime.UtcNow;
                
                var context = new DatabaseContext();
                var newId = await context.AddPhotoAsync(photo);
                photo.PhotoId = newId;
                _errorHandler.LogInfo($"Added photo: {photo.FileName} for listing {photo.ListingId}");
                return true;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error adding photo: {photo?.FileName}");
                return false;
            }
        }

        public async Task<bool> DeletePhotoAsync(int photoId)
        {
            try
            {
                var context = new DatabaseContext();
                await context.DeletePhotoAsync(photoId);
                _errorHandler.LogInfo($"Deleted photo with ID: {photoId}");
                return true;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error deleting photo with ID: {photoId}");
                return false;
            }
        }

        public async Task<bool> SetPrimaryPhotoAsync(int listingId, int photoId)
        {
            try
            {
                var context = new DatabaseContext();
                await context.SetPrimaryPhotoAsync(listingId, photoId);
                _errorHandler.LogInfo($"Set photo {photoId} as primary for listing {listingId}");
                return true;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error setting primary photo {photoId} for listing {listingId}");
                return false;
            }
        }

        // Category filtering methods - moved from HomeController
        public async Task<IEnumerable<Listing>> GetSmartWatchListingsAsync()
        {
            try
            {
                var allListings = await GetAllListingsAsync();
                var smartWatchListings = allListings
                    .Where(l => l.Title.ToLower().Contains("smart") || l.Description.ToLower().Contains("smart"))
                    .ToList();
                
                _errorHandler.LogInfo($"Retrieved {smartWatchListings.Count()} smart watch listings");
                return smartWatchListings;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "Error getting smart watch listings");
                return Enumerable.Empty<Listing>();
            }
        }

        public async Task<IEnumerable<Listing>> GetSportWatchListingsAsync()
        {
            try
            {
                var allListings = await GetAllListingsAsync();
                var sportWatchListings = allListings
                    .Where(l => l.Title.ToLower().Contains("sport") || l.Description.ToLower().Contains("sport") || 
                               l.Title.ToLower().Contains("casio") || l.Title.ToLower().Contains("g-shock"))
                    .ToList();
                
                _errorHandler.LogInfo($"Retrieved {sportWatchListings.Count()} sport watch listings");
                return sportWatchListings;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "Error getting sport watch listings");
                return Enumerable.Empty<Listing>();
            }
        }

        public async Task<IEnumerable<Listing>> GetLuxuryWatchListingsAsync()
        {
            try
            {
                var allListings = await GetAllListingsAsync();
                var luxuryWatchListings = allListings
                    .Where(l => l.Title.ToLower().Contains("luxury") || l.Description.ToLower().Contains("luxury") ||
                               l.Title.ToLower().Contains("cartier") || l.Price > 1000)
                    .ToList();
                
                _errorHandler.LogInfo($"Retrieved {luxuryWatchListings.Count()} luxury watch listings");
                return luxuryWatchListings;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "Error getting luxury watch listings");
                return Enumerable.Empty<Listing>();
            }
        }

        // Watch type classification method - moved from HomeController
        public string GetWatchType(string title)
        {
            try
            {
                if (string.IsNullOrEmpty(title))
                {
                    _errorHandler.LogWarning("GetWatchType called with null or empty title");
                    return "unknown";
                }

                title = title.ToLower();
                
                if (title.Contains("smart") || title.Contains("apple"))
                {
                    _errorHandler.LogInfo($"Classified watch as 'smart': {title}");
                    return "smart";
                }
                if (title.Contains("sport") || title.Contains("casio") || title.Contains("g-shock"))
                {
                    _errorHandler.LogInfo($"Classified watch as 'sport': {title}");
                    return "sport";
                }
                if (title.Contains("luxury") || title.Contains("cartier"))
                {
                    _errorHandler.LogInfo($"Classified watch as 'luxury': {title}");
                    return "luxury";
                }

                _errorHandler.LogInfo($"Classified watch as 'general': {title}");
                return "general";
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error classifying watch type for title: {title}");
                return "unknown";
            }
        }
    }
} 