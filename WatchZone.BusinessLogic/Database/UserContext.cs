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
        public UserContext() : 
            base("name=WatchZone")
        {
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

        public virtual DbSet<UDbTable> Users { get; set; }

        public bool TestDatabaseConnection()
        {
            try
            {
                // Test if we can connect to the database
                if (!Database.Exists())
                {
                    System.Diagnostics.Debug.WriteLine("Database does not exist!");
                    return false;
                }

                // Test if we can query the Users table
                var userCount = Users.Count();
                System.Diagnostics.Debug.WriteLine($"Successfully connected to database. User count: {userCount}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database connection test failed: {ex.Message}");
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error verifying user: {ex.Message}");
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding user: {ex.Message}");
                return false;
            }
        }

        public override int SaveChanges()
        {
            try
            {
                // Force the changes to be saved
                var result = base.SaveChanges();
                
                // Verify the save was successful
                if (result > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Successfully saved {result} changes to the database.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No changes were saved to the database.");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving changes: {ex.Message}");
                throw;
            }
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
