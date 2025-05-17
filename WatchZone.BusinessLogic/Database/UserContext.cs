using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchZone.Domain.Entities.User;

namespace WatchZone.BusinessLogic.Database
{
    class UserContext : DbContext
    {
        public UserContext() : 
            base("name=eUseControl")
        {
        }

        public virtual DbSet<UDbTable> Users { get; set; }
    }
}
