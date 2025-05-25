using System.Collections.Generic;
using System.Threading.Tasks;
using WatchZone.Domain.Model;

namespace WatchZone.BusinessLogic.Interface
{
    public interface IListingService
    {
        Task<IEnumerable<Listing>> GetAllListingsAsync();
        Task<Listing> GetListingByIdAsync(int id);
        Task<bool> CreateListingAsync(Listing listing);
        Task<bool> UpdateListingAsync(Listing listing);
        Task<bool> DeleteListingAsync(int id);
        Task<IEnumerable<Listing>> GetListingsByUserIdAsync(int userId);
        Task<bool> UserCanEditListing(int listingId, int userId, bool isAdmin);
        
        // Photo management methods
        Task<IEnumerable<ListingPhoto>> GetPhotosByListingIdAsync(int listingId);
        Task<ListingPhoto> GetPhotoByIdAsync(int photoId);
        Task<bool> AddPhotoAsync(ListingPhoto photo);
        Task<bool> DeletePhotoAsync(int photoId);
        Task<bool> SetPrimaryPhotoAsync(int listingId, int photoId);
    }
} 