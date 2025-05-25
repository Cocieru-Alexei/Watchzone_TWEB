using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchZone.BusinessLogic.Interface;
using WatchZone.Domain.Database;
using WatchZone.Domain.Model;

namespace WatchZone.BusinessLogic.BL_Struct
{
    public class ReviewServiceBL : IReviewService
    {
        private readonly IErrorHandler _errorHandler;

        public ReviewServiceBL(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public async Task<bool> CanUserReviewOrderItemAsync(int userId, int orderItemId)
        {
            try
            {
                var context = new DatabaseContext();
                return await context.CanUserReviewOrderItemAsync(userId, orderItemId);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error checking if user {userId} can review order item {orderItemId}");
                return false;
            }
        }

        public async Task<Review> CreateReviewAsync(Review review)
        {
            try
            {
                if (review == null)
                {
                    _errorHandler.LogWarning("Attempted to create null review");
                    return null;
                }

                var context = new DatabaseContext();
                var createdReview = await context.CreateReviewAsync(review);
                _errorHandler.LogInfo($"Created review for order item {review.OrderItemId} by user {review.ReviewerId}");
                return createdReview;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error creating review for order item {review?.OrderItemId}");
                return null;
            }
        }

        public async Task<Review> UpdateReviewAsync(Review review)
        {
            try
            {
                if (review == null)
                {
                    _errorHandler.LogWarning("Attempted to update null review");
                    return null;
                }

                var context = new DatabaseContext();
                var updatedReview = await context.UpdateReviewAsync(review);
                _errorHandler.LogInfo($"Updated review {review.ReviewId} by user {review.ReviewerId}");
                return updatedReview;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error updating review {review?.ReviewId}");
                return null;
            }
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
        {
            try
            {
                var context = new DatabaseContext();
                var deleted = await context.DeleteReviewAsync(reviewId, userId);
                if (deleted)
                {
                    _errorHandler.LogInfo($"Deleted review {reviewId} by user {userId}");
                }
                return deleted;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error deleting review {reviewId} by user {userId}");
                return false;
            }
        }

        public async Task<Review> GetReviewByIdAsync(int reviewId)
        {
            try
            {
                var context = new DatabaseContext();
                return await context.GetReviewByIdAsync(reviewId);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error getting review with ID: {reviewId}");
                return null;
            }
        }

        public async Task<Review> GetReviewByOrderItemIdAsync(int orderItemId)
        {
            try
            {
                var context = new DatabaseContext();
                return await context.GetReviewByOrderItemIdAsync(orderItemId);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error getting review for order item: {orderItemId}");
                return null;
            }
        }

        public async Task<List<Review>> GetReviewsBySellerIdAsync(int sellerId)
        {
            try
            {
                var context = new DatabaseContext();
                return await context.GetReviewsBySellerIdAsync(sellerId);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error getting reviews for seller: {sellerId}");
                return new List<Review>();
            }
        }

        public async Task<List<Review>> GetReviewsByReviewerIdAsync(int reviewerId)
        {
            try
            {
                var context = new DatabaseContext();
                return await context.GetReviewsByReviewerIdAsync(reviewerId);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error getting reviews by reviewer: {reviewerId}");
                return new List<Review>();
            }
        }

        public async Task<List<Review>> GetReviewsAboutSellerAsync(int sellerId)
        {
            try
            {
                var context = new DatabaseContext();
                return await context.GetReviewsAboutSellerAsync(sellerId);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error getting reviews about seller: {sellerId}");
                return new List<Review>();
            }
        }

        public async Task<ReviewSummary> GetReviewSummaryBySellerIdAsync(int sellerId)
        {
            try
            {
                var context = new DatabaseContext();
                return await context.GetReviewSummaryBySellerIdAsync(sellerId);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error getting review summary for seller: {sellerId}");
                return new ReviewSummary();
            }
        }

        public async Task<List<OrderItem>> GetReviewableOrderItemsAsync(int userId)
        {
            try
            {
                var context = new DatabaseContext();
                return await context.GetReviewableOrderItemsAsync(userId);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error getting reviewable order items for user: {userId}");
                return new List<OrderItem>();
            }
        }
    }
} 