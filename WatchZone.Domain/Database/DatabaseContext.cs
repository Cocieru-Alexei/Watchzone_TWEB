using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchZone.Domain.Model;

namespace WatchZone.Domain.Database
{
    public class DatabaseContext
    {
        private readonly string _connectionString = "Server=localhost;Database=WatchZone;Trusted_Connection=True;";

        public async Task<Listing> GetListingByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT * FROM Listings WHERE Listings_Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Listing
                            {
                                Listings_Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Description = reader.GetString(2),
                                Price = reader.GetDecimal(3),
                                CreatedAt = reader.GetDateTime(4),
                                UserId = reader.GetInt32(5),
                                ImageUrl = reader.IsDBNull(6) ? null : reader.GetString(6)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<List<Listing>> GetAllListingsAsync()
        {
            var listings = new List<Listing>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT * FROM Listings", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            listings.Add(new Listing
                            {
                                Listings_Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Description = reader.GetString(2),
                                Price = reader.GetDecimal(3),
                                CreatedAt = reader.GetDateTime(4),
                                UserId = reader.GetInt32(5),
                                ImageUrl = reader.IsDBNull(6) ? null : reader.GetString(6)
                            });
                        }
                    }
                }
            }
            return listings;
        }

        public async Task<int> CreateListingAsync(Listing listing)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    @"INSERT INTO Listings (Title, Description, Price, CreatedAt, UserId, ImageUrl) 
                      VALUES (@Title, @Description, @Price, @CreatedAt, @UserId, @ImageUrl);
                      SELECT SCOPE_IDENTITY();", connection))
                {
                    command.Parameters.AddWithValue("@Title", listing.Title);
                    command.Parameters.AddWithValue("@Description", listing.Description);
                    command.Parameters.AddWithValue("@Price", listing.Price);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                    command.Parameters.AddWithValue("@UserId", listing.UserId);
                    command.Parameters.AddWithValue("@ImageUrl", (object)listing.ImageUrl ?? DBNull.Value);

                    return Convert.ToInt32(await command.ExecuteScalarAsync());
                }
            }
        }

        public async Task UpdateListingAsync(int listingsId, string title, string description, decimal price, string imageUrl)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    @"UPDATE Listings
                      SET Title = @Title, Description = @Description, Price = @Price, ImageUrl = @ImageUrl
                      WHERE Listings_Id = @Listings_Id", connection))
                {
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@Price", price);
                    command.Parameters.AddWithValue("@ImageUrl", (object)imageUrl ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Listings_Id", listingsId);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteListingAsync(int listingsId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "DELETE FROM Listings WHERE Listings_Id = @Listings_Id", connection))
                {
                    command.Parameters.AddWithValue("@Listings_Id", listingsId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
} 