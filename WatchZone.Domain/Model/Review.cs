using System;

namespace WatchZone.Domain.Model
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int OrderItemId { get; set; }
        public int ReviewerId { get; set; } // User who wrote the review (buyer)
        public int SellerId { get; set; } // User being reviewed (seller)
        public int ListingId { get; set; } // The listing that was purchased
        public int Rating { get; set; } // 1-5 stars
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public OrderItem OrderItem { get; set; }
        public Listing Listing { get; set; }
        
        // Additional properties for display purposes
        public string ReviewerName { get; set; }
        public string SellerName { get; set; }
        public string ListingTitle { get; set; }
    }
} 