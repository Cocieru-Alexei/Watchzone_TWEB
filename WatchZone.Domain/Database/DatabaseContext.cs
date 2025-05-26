using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
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

        // Order-related methods
        public async Task<int> CreateOrderAsync(Order order)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert order
                        int orderId;
                        using (var command = new SqlCommand(
                            @"INSERT INTO Orders (UserId, OrderDate, TotalAmount, OrderStatus, ShippingAddress, BillingAddress, PaymentMethod, PaymentStatus, Notes) 
                              VALUES (@UserId, @OrderDate, @TotalAmount, @OrderStatus, @ShippingAddress, @BillingAddress, @PaymentMethod, @PaymentStatus, @Notes);
                              SELECT SCOPE_IDENTITY();", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@UserId", order.UserId);
                            command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                            command.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                            command.Parameters.AddWithValue("@OrderStatus", order.OrderStatus);
                            command.Parameters.AddWithValue("@ShippingAddress", order.ShippingAddress);
                            command.Parameters.AddWithValue("@BillingAddress", order.BillingAddress);
                            command.Parameters.AddWithValue("@PaymentMethod", order.PaymentMethod);
                            command.Parameters.AddWithValue("@PaymentStatus", order.PaymentStatus);
                            command.Parameters.AddWithValue("@Notes", (object)order.Notes ?? DBNull.Value);

                            orderId = Convert.ToInt32(await command.ExecuteScalarAsync());
                        }

                        // Insert order items
                        foreach (var item in order.OrderItems)
                        {
                            using (var command = new SqlCommand(
                                @"INSERT INTO OrderItems (OrderId, ListingId, ProductName, UnitPrice, Quantity, TotalPrice, ProductImageUrl) 
                                  VALUES (@OrderId, @ListingId, @ProductName, @UnitPrice, @Quantity, @TotalPrice, @ProductImageUrl)", connection, transaction))
                            {
                                command.Parameters.AddWithValue("@OrderId", orderId);
                                command.Parameters.AddWithValue("@ListingId", item.ListingId);
                                command.Parameters.AddWithValue("@ProductName", item.ProductName);
                                command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                                command.Parameters.AddWithValue("@TotalPrice", item.TotalPrice);
                                command.Parameters.AddWithValue("@ProductImageUrl", (object)item.ProductImageUrl ?? DBNull.Value);

                                await command.ExecuteNonQueryAsync();
                            }
                        }

                        transaction.Commit();
                        return orderId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                Order order = null;
                
                // Get order details
                using (var command = new SqlCommand(
                    @"SELECT OrderId, UserId, OrderDate, TotalAmount, OrderStatus, ShippingAddress, BillingAddress, 
                             PaymentMethod, PaymentStatus, ShippedDate, DeliveredDate, TrackingNumber, Notes, CreatedAt, UpdatedAt 
                      FROM Orders WHERE OrderId = @OrderId", connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            order = new Order
                            {
                                OrderId = reader.GetInt32(0),
                                UserId = reader.GetInt32(1),
                                OrderDate = reader.GetDateTime(2),
                                TotalAmount = reader.GetDecimal(3),
                                OrderStatus = reader.GetString(4),
                                ShippingAddress = reader.GetString(5),
                                BillingAddress = reader.GetString(6),
                                PaymentMethod = reader.GetString(7),
                                PaymentStatus = reader.GetString(8),
                                ShippedDate = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9),
                                DeliveredDate = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                                TrackingNumber = reader.IsDBNull(11) ? null : reader.GetString(11),
                                Notes = reader.IsDBNull(12) ? null : reader.GetString(12)
                            };
                        }
                    }
                }

                if (order != null)
                {
                    // Get order items
                    using (var command = new SqlCommand(
                        @"SELECT OrderItemId, OrderId, ListingId, ProductName, UnitPrice, Quantity, TotalPrice, ProductImageUrl 
                          FROM OrderItems WHERE OrderId = @OrderId", connection))
                    {
                        command.Parameters.AddWithValue("@OrderId", orderId);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var orderItem = new OrderItem
                                {
                                    OrderItemId = reader.GetInt32(0),
                                    OrderId = reader.GetInt32(1),
                                    ListingId = reader.GetInt32(2),
                                    ProductName = reader.GetString(3),
                                    UnitPrice = reader.GetDecimal(4),
                                    Quantity = reader.GetInt32(5),
                                    TotalPrice = reader.GetDecimal(6),
                                    ProductImageUrl = reader.IsDBNull(7) ? null : reader.GetString(7)
                                };
                                order.OrderItems.Add(orderItem);
                            }
                        }
                    }
                }

                return order;
            }
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = new List<Order>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                // Get orders for user
                using (var command = new SqlCommand(
                    @"SELECT OrderId, UserId, OrderDate, TotalAmount, OrderStatus, ShippingAddress, BillingAddress, 
                             PaymentMethod, PaymentStatus, ShippedDate, DeliveredDate, TrackingNumber, Notes, CreatedAt, UpdatedAt 
                      FROM Orders WHERE UserId = @UserId ORDER BY OrderDate DESC", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var order = new Order
                            {
                                OrderId = reader.GetInt32(0),
                                UserId = reader.GetInt32(1),
                                OrderDate = reader.GetDateTime(2),
                                TotalAmount = reader.GetDecimal(3),
                                OrderStatus = reader.GetString(4),
                                ShippingAddress = reader.GetString(5),
                                BillingAddress = reader.GetString(6),
                                PaymentMethod = reader.GetString(7),
                                PaymentStatus = reader.GetString(8),
                                ShippedDate = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9),
                                DeliveredDate = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                                TrackingNumber = reader.IsDBNull(11) ? null : reader.GetString(11),
                                Notes = reader.IsDBNull(12) ? null : reader.GetString(12)
                            };
                            orders.Add(order);
                        }
                    }
                }

                // Get order items for each order
                foreach (var order in orders)
                {
                    using (var command = new SqlCommand(
                        @"SELECT OrderItemId, OrderId, ListingId, ProductName, UnitPrice, Quantity, TotalPrice, ProductImageUrl 
                          FROM OrderItems WHERE OrderId = @OrderId", connection))
                    {
                        command.Parameters.AddWithValue("@OrderId", order.OrderId);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var orderItem = new OrderItem
                                {
                                    OrderItemId = reader.GetInt32(0),
                                    OrderId = reader.GetInt32(1),
                                    ListingId = reader.GetInt32(2),
                                    ProductName = reader.GetString(3),
                                    UnitPrice = reader.GetDecimal(4),
                                    Quantity = reader.GetInt32(5),
                                    TotalPrice = reader.GetDecimal(6),
                                    ProductImageUrl = reader.IsDBNull(7) ? null : reader.GetString(7)
                                };
                                order.OrderItems.Add(orderItem);
                            }
                        }
                    }
                }
            }
            return orders;
        }

        public async Task UpdateOrderStatusAsync(int orderId, string orderStatus, string paymentStatus = null, string trackingNumber = null, DateTime? shippedDate = null, DateTime? deliveredDate = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var sql = "UPDATE Orders SET OrderStatus = @OrderStatus, UpdatedAt = GETUTCDATE()";
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@OrderStatus", orderStatus),
                    new SqlParameter("@OrderId", orderId)
                };

                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    sql += ", PaymentStatus = @PaymentStatus";
                    parameters.Add(new SqlParameter("@PaymentStatus", paymentStatus));
                }

                if (!string.IsNullOrEmpty(trackingNumber))
                {
                    sql += ", TrackingNumber = @TrackingNumber";
                    parameters.Add(new SqlParameter("@TrackingNumber", trackingNumber));
                }

                if (shippedDate.HasValue)
                {
                    sql += ", ShippedDate = @ShippedDate";
                    parameters.Add(new SqlParameter("@ShippedDate", shippedDate.Value));
                }

                if (deliveredDate.HasValue)
                {
                    sql += ", DeliveredDate = @DeliveredDate";
                    parameters.Add(new SqlParameter("@DeliveredDate", deliveredDate.Value));
                }

                sql += " WHERE OrderId = @OrderId";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // Check if a listing has been sold
        public async Task<bool> IsListingSoldAsync(int listingId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "SELECT COUNT(*) FROM OrderItems WHERE ListingId = @ListingId", connection))
                {
                    command.Parameters.AddWithValue("@ListingId", listingId);
                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }

        // Get all available (unsold) listings
        public async Task<List<Listing>> GetAvailableListingsAsync()
        {
            var allListings = await GetAllListingsAsync();
            var availableListings = new List<Listing>();

            foreach (var listing in allListings)
            {
                var isSold = await IsListingSoldAsync(listing.Listings_Id);
                if (!isSold)
                {
                    availableListings.Add(listing);
                }
            }

            return availableListings;
        }

        // Search listings by title or description (only available/unsold listings)
        public async Task<List<Listing>> SearchListingsAsync(string searchQuery)
        {
            var listings = new List<Listing>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    SELECT l.* 
                    FROM Listings l
                    WHERE (l.Title LIKE @SearchQuery OR l.Description LIKE @SearchQuery)
                    AND NOT EXISTS (
                        SELECT 1 FROM OrderItems oi WHERE oi.ListingId = l.Listings_Id
                    )
                    ORDER BY l.CreatedAt DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SearchQuery", $"%{searchQuery}%");
                    
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

        // Review-related methods
        public async Task<bool> CanUserReviewOrderItemAsync(int userId, int orderItemId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    SELECT COUNT(1) 
                    FROM OrderItems oi
                    INNER JOIN Orders o ON oi.OrderId = o.OrderId
                    WHERE oi.OrderItemId = @OrderItemId 
                    AND o.UserId = @UserId 
                    AND NOT EXISTS (
                        SELECT 1 FROM Reviews r WHERE r.OrderItemId = @OrderItemId
                    )";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderItemId", orderItemId);
                    command.Parameters.AddWithValue("@UserId", userId);
                    
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result) > 0;
                }
            }
        }

        public async Task<Review> CreateReviewAsync(Review review)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    INSERT INTO Reviews (OrderItemId, ReviewerId, SellerId, ListingId, Rating, Comment, CreatedAt)
                    OUTPUT INSERTED.ReviewId
                    VALUES (@OrderItemId, @ReviewerId, @SellerId, @ListingId, @Rating, @Comment, @CreatedAt)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderItemId", review.OrderItemId);
                    command.Parameters.AddWithValue("@ReviewerId", review.ReviewerId);
                    command.Parameters.AddWithValue("@SellerId", review.SellerId);
                    command.Parameters.AddWithValue("@ListingId", review.ListingId);
                    command.Parameters.AddWithValue("@Rating", review.Rating);
                    command.Parameters.AddWithValue("@Comment", review.Comment ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                    
                    var reviewId = await command.ExecuteScalarAsync();
                    review.ReviewId = Convert.ToInt32(reviewId);
                    review.CreatedAt = DateTime.UtcNow;
                    
                    return review;
                }
            }
        }

        public async Task<Review> UpdateReviewAsync(Review review)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    UPDATE Reviews 
                    SET Rating = @Rating, Comment = @Comment, UpdatedAt = @UpdatedAt
                    WHERE ReviewId = @ReviewId AND ReviewerId = @ReviewerId";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ReviewId", review.ReviewId);
                    command.Parameters.AddWithValue("@ReviewerId", review.ReviewerId);
                    command.Parameters.AddWithValue("@Rating", review.Rating);
                    command.Parameters.AddWithValue("@Comment", review.Comment ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                    
                    await command.ExecuteNonQueryAsync();
                    review.UpdatedAt = DateTime.UtcNow;
                    
                    return review;
                }
            }
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = "DELETE FROM Reviews WHERE ReviewId = @ReviewId AND ReviewerId = @UserId";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ReviewId", reviewId);
                    command.Parameters.AddWithValue("@UserId", userId);
                    
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<Review> GetReviewByIdAsync(int reviewId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    SELECT ReviewId, OrderItemId, ReviewerId, SellerId, ListingId, Rating, Comment, CreatedAt, UpdatedAt
                    FROM Reviews 
                    WHERE ReviewId = @ReviewId";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ReviewId", reviewId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapReviewFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }

        public async Task<Review> GetReviewByOrderItemIdAsync(int orderItemId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    SELECT ReviewId, OrderItemId, ReviewerId, SellerId, ListingId, Rating, Comment, CreatedAt, UpdatedAt
                    FROM Reviews 
                    WHERE OrderItemId = @OrderItemId";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderItemId", orderItemId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapReviewFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }

        public async Task<List<Review>> GetReviewsBySellerIdAsync(int sellerId)
        {
            var reviews = new List<Review>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    SELECT r.ReviewId, r.OrderItemId, r.ReviewerId, r.SellerId, r.ListingId, r.Rating, r.Comment, r.CreatedAt, r.UpdatedAt,
                           u.Username as ReviewerName, l.Title as ListingTitle
                    FROM Reviews r
                    INNER JOIN UDbTable u ON r.ReviewerId = u.Id
                    INNER JOIN Listings l ON r.ListingId = l.Listings_Id
                    WHERE r.SellerId = @SellerId
                    ORDER BY r.CreatedAt DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SellerId", sellerId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var review = MapReviewFromReader(reader);
                            review.ReviewerName = reader.GetString(9);
                            review.ListingTitle = reader.GetString(10);
                            reviews.Add(review);
                        }
                    }
                }
            }
            
            return reviews;
        }

        public async Task<List<Review>> GetReviewsByReviewerIdAsync(int reviewerId)
        {
            var reviews = new List<Review>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    SELECT r.ReviewId, r.OrderItemId, r.ReviewerId, r.SellerId, r.ListingId, r.Rating, r.Comment, r.CreatedAt, r.UpdatedAt,
                           u.Username as SellerName, l.Title as ListingTitle
                    FROM Reviews r
                    INNER JOIN UDbTable u ON r.SellerId = u.Id
                    INNER JOIN Listings l ON r.ListingId = l.Listings_Id
                    WHERE r.ReviewerId = @ReviewerId
                    ORDER BY r.CreatedAt DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ReviewerId", reviewerId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var review = MapReviewFromReader(reader);
                            review.SellerName = reader.GetString(9);
                            review.ListingTitle = reader.GetString(10);
                            reviews.Add(review);
                        }
                    }
                }
            }
            
            return reviews;
        }

        public async Task<List<Review>> GetReviewsAboutSellerAsync(int sellerId)
        {
            var reviews = new List<Review>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    SELECT r.ReviewId, r.OrderItemId, r.ReviewerId, r.SellerId, r.ListingId, r.Rating, r.Comment, r.CreatedAt, r.UpdatedAt,
                           u.Username as ReviewerName, l.Title as ListingTitle
                    FROM Reviews r
                    INNER JOIN UDbTable u ON r.ReviewerId = u.Id
                    INNER JOIN Listings l ON r.ListingId = l.Listings_Id
                    WHERE r.SellerId = @SellerId
                    ORDER BY r.CreatedAt DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SellerId", sellerId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var review = MapReviewFromReader(reader);
                            review.ReviewerName = reader.GetString(9);
                            review.ListingTitle = reader.GetString(10);
                            reviews.Add(review);
                        }
                    }
                }
            }
            
            return reviews;
        }

        public async Task<ReviewSummary> GetReviewSummaryBySellerIdAsync(int sellerId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    SELECT 
                        COUNT(*) as TotalReviews,
                        AVG(CAST(Rating as FLOAT)) as AverageRating,
                        SUM(CASE WHEN Rating = 5 THEN 1 ELSE 0 END) as FiveStars,
                        SUM(CASE WHEN Rating = 4 THEN 1 ELSE 0 END) as FourStars,
                        SUM(CASE WHEN Rating = 3 THEN 1 ELSE 0 END) as ThreeStars,
                        SUM(CASE WHEN Rating = 2 THEN 1 ELSE 0 END) as TwoStars,
                        SUM(CASE WHEN Rating = 1 THEN 1 ELSE 0 END) as OneStar
                    FROM Reviews 
                    WHERE SellerId = @SellerId";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SellerId", sellerId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ReviewSummary
                            {
                                SellerId = sellerId,
                                TotalReviews = reader.GetInt32(0),
                                AverageRating = reader.IsDBNull(1) ? 0.0 : reader.GetDouble(1),
                                FiveStarCount = reader.GetInt32(2),
                                FourStarCount = reader.GetInt32(3),
                                ThreeStarCount = reader.GetInt32(4),
                                TwoStarCount = reader.GetInt32(5),
                                OneStarCount = reader.GetInt32(6)
                            };
                        }
                    }
                }
            }
            
            return new ReviewSummary { SellerId = sellerId };
        }

        public async Task<List<OrderItem>> GetReviewableOrderItemsAsync(int userId)
        {
            var orderItems = new List<OrderItem>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    SELECT oi.OrderItemId, oi.OrderId, oi.ListingId, oi.ProductName, oi.UnitPrice, oi.Quantity, oi.TotalPrice, oi.ProductImageUrl,
                           o.OrderDate, l.UserId as SellerId
                    FROM OrderItems oi
                    INNER JOIN Orders o ON oi.OrderId = o.OrderId
                    INNER JOIN Listings l ON oi.ListingId = l.Listings_Id
                    WHERE o.UserId = @UserId 
                    AND NOT EXISTS (
                        SELECT 1 FROM Reviews r WHERE r.OrderItemId = oi.OrderItemId
                    )
                    ORDER BY o.OrderDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orderItems.Add(new OrderItem
                            {
                                OrderItemId = reader.GetInt32(0),
                                OrderId = reader.GetInt32(1),
                                ListingId = reader.GetInt32(2),
                                ProductName = reader.GetString(3),
                                UnitPrice = reader.GetDecimal(4),
                                Quantity = reader.GetInt32(5),
                                TotalPrice = reader.GetDecimal(6),
                                ProductImageUrl = reader.IsDBNull(7) ? null : reader.GetString(7)
                            });
                        }
                    }
                }
            }
            
            return orderItems;
        }

        private Review MapReviewFromReader(SqlDataReader reader)
        {
            return new Review
            {
                ReviewId = reader.GetInt32(0),
                OrderItemId = reader.GetInt32(1),
                ReviewerId = reader.GetInt32(2),
                SellerId = reader.GetInt32(3),
                ListingId = reader.GetInt32(4),
                Rating = reader.GetInt32(5),
                Comment = reader.IsDBNull(6) ? null : reader.GetString(6),
                CreatedAt = reader.GetDateTime(7),
                UpdatedAt = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8)
            };
        }
    }
} 