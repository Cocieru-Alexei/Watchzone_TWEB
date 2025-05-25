using System;
using System.Collections.Generic;

namespace WatchZone.Domain.Model
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } // Pending, Confirmed, Shipped, Delivered, Cancelled
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; } // Pending, Paid, Failed, Refunded
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string TrackingNumber { get; set; }
        public string Notes { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
} 