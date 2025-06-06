using System;
using System.Collections.Generic;

namespace WatchZone.Domain.Model
{
    public class Listing
    {
        public int Listings_Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public string ImageUrl { get; set; } // Keep for backward compatibility
        public List<ListingPhoto> Photos { get; set; } = new List<ListingPhoto>();
    }
} 