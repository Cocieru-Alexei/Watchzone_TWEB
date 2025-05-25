using System;

namespace WatchZone.Domain.Model
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ListingId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string ProductImageUrl { get; set; }
        
        // Navigation properties
        public Order Order { get; set; }
        public Listing Listing { get; set; }
    }
} 