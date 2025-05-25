using System.Collections.Generic;
using System.Threading.Tasks;
using WatchZone.Domain.Model;

namespace WatchZone.BusinessLogic.Interface
{
    public interface IReviewService
    {
        Task<bool> CanUserReviewOrderItemAsync(int userId, int orderItemId);
        Task<Review> CreateReviewAsync(Review review);
        Task<Review> UpdateReviewAsync(Review review);
        Task<bool> DeleteReviewAsync(int reviewId, int userId);
        Task<Review> GetReviewByIdAsync(int reviewId);
        Task<Review> GetReviewByOrderItemIdAsync(int orderItemId);
        Task<List<Review>> GetReviewsBySellerIdAsync(int sellerId);
        Task<List<Review>> GetReviewsByReviewerIdAsync(int reviewerId);
        Task<List<Review>> GetReviewsAboutSellerAsync(int sellerId);
        Task<ReviewSummary> GetReviewSummaryBySellerIdAsync(int sellerId);
        Task<List<OrderItem>> GetReviewableOrderItemsAsync(int userId);
    }
} 