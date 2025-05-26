using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchZone.Domain.Entities.User;
using WatchZone.Domain.Enums;
using WatchZone.Helper;

namespace WatchZone.BusinessLogic.Database
{
    public class UserContext : DbContext
    {
        static UserContext()
        {
            // Set database initialization strategy to create if not exists (handles existing databases)
            System.Data.Entity.Database.SetInitializer(new CreateDatabaseIfNotExists<UserContext>());
        }

        public UserContext() : 
            base("name=WatchZone")
        {
            try
            {
                // Force database creation/initialization
                Database.Initialize(force: false);
                
                // Seed admin user if none exists
                if (!Users.Any())
                {
                    var adminUser = new UDbTable
                    {
                        Username = "admin",
                        Password = LoginUtility.GenHash("admin123"),
                        Email = "admin@watchzone.com",
                        Level = URole.Admin,
                        LastLogin = DateTime.UtcNow,
                        LasIp = "127.0.0.1"
                    };
                    Users.Add(adminUser);
                    SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // If there's a schema issue, try to recreate the database
                if (ex.Message.Contains("model compatibility") || ex.Message.Contains("Invalid column"))
                {
                    Database.Delete();
                    Database.Create();
                    
                    // Re-seed admin user
                    var adminUser = new UDbTable
                    {
                        Username = "admin",
                        Password = LoginUtility.GenHash("admin123"),
                        Email = "admin@watchzone.com",
                        Level = URole.Admin,
                        LastLogin = DateTime.UtcNow,
                        LasIp = "127.0.0.1"
                    };
                    Users.Add(adminUser);
                    SaveChanges();
                }
                else
                {
                    throw;
                }
            }
        }

        public virtual DbSet<UDbTable> Users { get; set; }

        public bool TestDatabaseConnection()
        {
            try
            {
                if (!Database.Exists())
                {
                    return false;
                }

                var userCount = Users.Count();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool VerifyUserExists(string username)
        {
            try
            {
                var user = Users.FirstOrDefault(u => u.Username == username);
                return user != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddUser(UDbTable user)
        {
            try
            {
                Users.Add(user);
                var result = SaveChanges();
                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Configure the UDbTable entity
            modelBuilder.Entity<UDbTable>()
                .ToTable("UDbTable");  // Explicitly set the table name

            base.OnModelCreating(modelBuilder);
        }
    }
}
