using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;
using WatchZone.BusinessLogic.Interface;
using WatchZone.BusinessLogic.Services;
using WatchZone.Domain.Model;
using WatchZone.Web.Models;

namespace WatchZone.Web.Controllers
{
    public class ReviewController : BaseController
    {
        private readonly IReviewService _reviewService;

        public ReviewController()
        {
            // Use Business Logic pattern consistent with other controllers
            var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
            _reviewService = businessLogic.GetReviewService();
        }

        // GET: Review/MyReviews
        public async Task<ActionResult> MyReviews()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var reviewsIWrote = await _reviewService.GetReviewsByReviewerIdAsync(userId.Value);
            var reviewsAboutMe = await _reviewService.GetReviewsAboutSellerAsync(userId.Value);
            var myReviewSummary = await _reviewService.GetReviewSummaryBySellerIdAsync(userId.Value);
            
            ViewBag.ReviewsIWrote = reviewsIWrote;
            ViewBag.ReviewsAboutMe = reviewsAboutMe;
            ViewBag.MyReviewSummary = myReviewSummary;
            
            return View();
        }

        // GET: Review/ReviewableItems
        public async Task<ActionResult> ReviewableItems()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var orderItems = await _reviewService.GetReviewableOrderItemsAsync(userId.Value);
            
            return View(orderItems);
        }

        // GET: Review/Create/5
        public async Task<ActionResult> Create(int orderItemId)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var canReview = await _reviewService.CanUserReviewOrderItemAsync(userId.Value, orderItemId);
            
            if (!canReview)
            {
                TempData["ErrorMessage"] = "You cannot review this item.";
                return RedirectToAction("ReviewableItems");
            }

            // Get order item details for the form
            var orderItems = await _reviewService.GetReviewableOrderItemsAsync(userId.Value);
            var orderItem = orderItems.Find(oi => oi.OrderItemId == orderItemId);
            
            if (orderItem == null)
            {
                TempData["ErrorMessage"] = "Order item not found.";
                return RedirectToAction("ReviewableItems");
            }

            ViewBag.OrderItem = orderItem;
            return View();
        }

        // POST: Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(int orderItemId, int rating, string comment)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var canReview = await _reviewService.CanUserReviewOrderItemAsync(userId.Value, orderItemId);
            
            if (!canReview)
            {
                TempData["ErrorMessage"] = "You cannot review this item.";
                return RedirectToAction("ReviewableItems");
            }

            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "Rating must be between 1 and 5 stars.";
                return RedirectToAction("Create", new { orderItemId });
            }

            try
            {
                // Get seller ID from the listing
                var orderItems = await _reviewService.GetReviewableOrderItemsAsync(userId.Value);
                var orderItem = orderItems.Find(oi => oi.OrderItemId == orderItemId);
                
                if (orderItem == null)
                {
                    TempData["ErrorMessage"] = "Order item not found.";
                    return RedirectToAction("ReviewableItems");
                }

                // Get seller ID from listing using Business Logic
                var sellerId = await GetSellerIdFromListingAsync(orderItem.ListingId);

                var review = new Review
                {
                    OrderItemId = orderItemId,
                    ReviewerId = userId.Value,
                    SellerId = sellerId,
                    ListingId = orderItem.ListingId,
                    Rating = rating,
                    Comment = comment
                };

                await _reviewService.CreateReviewAsync(review);
                TempData["SuccessMessage"] = "Review submitted successfully!";
                
                return RedirectToAction("MyReviews");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting your review.";
                return RedirectToAction("Create", new { orderItemId });
            }
        }

        // GET: Review/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var review = await _reviewService.GetReviewByIdAsync(id);
            
            if (review == null || review.ReviewerId != userId.Value)
            {
                TempData["ErrorMessage"] = "Review not found or you don't have permission to edit it.";
                return RedirectToAction("MyReviews");
            }

            return View(review);
        }

        // POST: Review/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, int rating, string comment)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var review = await _reviewService.GetReviewByIdAsync(id);
            
            if (review == null || review.ReviewerId != userId.Value)
            {
                TempData["ErrorMessage"] = "Review not found or you don't have permission to edit it.";
                return RedirectToAction("MyReviews");
            }

            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "Rating must be between 1 and 5 stars.";
                return RedirectToAction("Edit", new { id });
            }

            try
            {
                review.Rating = rating;
                review.Comment = comment;
                
                await _reviewService.UpdateReviewAsync(review);
                TempData["SuccessMessage"] = "Review updated successfully!";
                
                return RedirectToAction("MyReviews");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating your review.";
                return RedirectToAction("Edit", new { id });
            }
        }

        // POST: Review/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }
            
            try
            {
                var deleted = await _reviewService.DeleteReviewAsync(id, userId.Value);
                
                if (deleted)
                {
                    TempData["SuccessMessage"] = "Review deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Review not found or you don't have permission to delete it.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting your review.";
            }
            
            return RedirectToAction("MyReviews");
        }

        // GET: Review/SellerReviews/5
        public async Task<ActionResult> SellerReviews(int sellerId)
        {
            var reviews = await _reviewService.GetReviewsBySellerIdAsync(sellerId);
            var reviewSummary = await _reviewService.GetReviewSummaryBySellerIdAsync(sellerId);
            
            ViewBag.ReviewSummary = reviewSummary;
            ViewBag.SellerId = sellerId;
            
            return View(reviews);
        }

        private async Task<int> GetSellerIdFromListingAsync(int listingId)
        {
            // Use Business Logic pattern to get seller ID
            try
            {
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var listingService = businessLogic.GetListingService();
                var listing = await listingService.GetListingByIdAsync(listingId);
                return listing?.UserId ?? 0;
            }
            catch (Exception ex)
            {
                // Log error and return 0 as fallback
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var errorHandler = businessLogic.GetErrorHandler();
                errorHandler.LogError(ex, $"Error getting seller ID for listing {listingId}");
                return 0;
            }
        }
    }
} 