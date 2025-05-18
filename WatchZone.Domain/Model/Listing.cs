using System;

namespace WatchZone.Domain.Model
{
    public class Listing
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public string ImageUrl { get; set; }
    }
} 