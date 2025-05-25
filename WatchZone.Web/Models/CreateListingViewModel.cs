using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Collections.Generic;

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

        public string ImageUrl { get; set; } // Keep for backward compatibility

        [Display(Name = "Upload Photos")]
        public List<HttpPostedFileBase> Photos { get; set; } = new List<HttpPostedFileBase>();
    }
} 