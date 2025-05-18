using System.ComponentModel.DataAnnotations;

namespace WatchZone.Web.Models
{
    public class CreateListingViewModel
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; }
    }
} 