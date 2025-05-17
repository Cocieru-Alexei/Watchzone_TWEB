using System;
using System.Collections.Generic;
using System.Data.Entity;
using WatchZone.Domain.Entities.User;

namespace WatchZone.BusinessLogic.Database
{
    public class SessionContext : DbContext
    {
        public SessionContext() : base("name=WatchZone")
        {
        }

        public virtual DbSet<Session> Sessions { get; set; }
    }
}
