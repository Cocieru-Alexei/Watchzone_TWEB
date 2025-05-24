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
    }
} 