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
    }
} 