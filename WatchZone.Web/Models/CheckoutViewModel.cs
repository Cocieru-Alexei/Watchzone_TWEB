using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WatchZone.Domain.Model.Cart;

namespace WatchZone.Web.Models
{
    public class CheckoutViewModel
    {
        public Cart Cart { get; set; }
        
        [Required(ErrorMessage = "Shipping address is required")]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; }
        
        [Required(ErrorMessage = "Billing address is required")]
        [Display(Name = "Billing Address")]
        public string BillingAddress { get; set; }
        
        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }
        
        [Display(Name = "Order Notes")]
        public string Notes { get; set; }
        
        public decimal TotalAmount { get; set; }
        
        public List<string> PaymentMethods { get; set; } = new List<string>
        {
            "Credit Card",
            "PayPal",
            "Bank Transfer",
            "Cash on Delivery"
        };
    }
} 