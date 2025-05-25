using System.Collections.Generic;
using WatchZone.Domain.Entities.User;
using WatchZone.Domain.Model;

namespace WatchZone.Web.Models.Profile
{
    public class ProfileViewModel
    {
        public UserMinimal User { get; set; }
        public IList<Listing> UserListings { get; set; } = new List<Listing>();
    }
} 