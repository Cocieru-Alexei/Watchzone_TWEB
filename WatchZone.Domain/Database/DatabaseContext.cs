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

        // Photo-related methods
        public async Task<List<ListingPhoto>> GetPhotosByListingIdAsync(int listingId)
        {
            var photos = new List<ListingPhoto>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "SELECT PhotoId, ListingId, FileName, FilePath, IsPrimary, UploadedAt, DisplayOrder FROM ListingPhotos WHERE ListingId = @ListingId ORDER BY DisplayOrder", connection))
                {
                    command.Parameters.AddWithValue("@ListingId", listingId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            photos.Add(new ListingPhoto
                            {
                                PhotoId = reader.GetInt32(0),
                                ListingId = reader.GetInt32(1),
                                FileName = reader.GetString(2),
                                FilePath = reader.GetString(3),
                                IsPrimary = reader.GetBoolean(4),
                                UploadedAt = reader.GetDateTime(5),
                                DisplayOrder = reader.GetInt32(6)
                            });
                        }
                    }
                }
            }
            return photos;
        }

        public async Task<ListingPhoto> GetPhotoByIdAsync(int photoId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "SELECT PhotoId, ListingId, FileName, FilePath, IsPrimary, UploadedAt, DisplayOrder FROM ListingPhotos WHERE PhotoId = @PhotoId", connection))
                {
                    command.Parameters.AddWithValue("@PhotoId", photoId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ListingPhoto
                            {
                                PhotoId = reader.GetInt32(0),
                                ListingId = reader.GetInt32(1),
                                FileName = reader.GetString(2),
                                FilePath = reader.GetString(3),
                                IsPrimary = reader.GetBoolean(4),
                                UploadedAt = reader.GetDateTime(5),
                                DisplayOrder = reader.GetInt32(6)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<int> AddPhotoAsync(ListingPhoto photo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    @"INSERT INTO ListingPhotos (ListingId, FileName, FilePath, IsPrimary, UploadedAt, DisplayOrder) 
                      VALUES (@ListingId, @FileName, @FilePath, @IsPrimary, @UploadedAt, @DisplayOrder);
                      SELECT SCOPE_IDENTITY();", connection))
                {
                    command.Parameters.AddWithValue("@ListingId", photo.ListingId);
                    command.Parameters.AddWithValue("@FileName", photo.FileName);
                    command.Parameters.AddWithValue("@FilePath", photo.FilePath);
                    command.Parameters.AddWithValue("@IsPrimary", photo.IsPrimary);
                    command.Parameters.AddWithValue("@UploadedAt", photo.UploadedAt);
                    command.Parameters.AddWithValue("@DisplayOrder", photo.DisplayOrder);

                    return Convert.ToInt32(await command.ExecuteScalarAsync());
                }
            }
        }

        public async Task DeletePhotoAsync(int photoId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "DELETE FROM ListingPhotos WHERE PhotoId = @PhotoId", connection))
                {
                    command.Parameters.AddWithValue("@PhotoId", photoId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeletePhotosByListingIdAsync(int listingId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "DELETE FROM ListingPhotos WHERE ListingId = @ListingId", connection))
                {
                    command.Parameters.AddWithValue("@ListingId", listingId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task SetPrimaryPhotoAsync(int listingId, int photoId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // First, set all photos for this listing to non-primary
                        using (var command1 = new SqlCommand(
                            "UPDATE ListingPhotos SET IsPrimary = 0 WHERE ListingId = @ListingId", connection, transaction))
                        {
                            command1.Parameters.AddWithValue("@ListingId", listingId);
                            await command1.ExecuteNonQueryAsync();
                        }

                        // Then set the specified photo as primary
                        using (var command2 = new SqlCommand(
                            "UPDATE ListingPhotos SET IsPrimary = 1 WHERE PhotoId = @PhotoId", connection, transaction))
                        {
                            command2.Parameters.AddWithValue("@PhotoId", photoId);
                            await command2.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
} 