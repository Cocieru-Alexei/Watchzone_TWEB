using System;

namespace WatchZone.Web.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int WatchId { get; set; }
        public string WatchName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public DateTime AddedDate { get; set; }
    }
} 