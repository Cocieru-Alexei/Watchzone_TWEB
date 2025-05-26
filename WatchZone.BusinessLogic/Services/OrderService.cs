using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchZone.Domain.Database;
using WatchZone.Domain.Model;
using WatchZone.BusinessLogic.Interface;

namespace WatchZone.BusinessLogic.Services
{
    public class OrderService
    {
        private readonly IErrorHandler _errorHandler;

        public OrderService(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public async Task<int> CreateOrderAsync(Order order)
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    var orderId = await context.CreateOrderAsync(order);
                    _errorHandler.LogInfo($"Created order with ID: {orderId}");
                    return orderId;
                }
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "Error creating order");
                throw new Exception($"Error creating order: {ex.Message}", ex);
            }
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    return await context.GetOrderByIdAsync(orderId);
                }
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error retrieving order {orderId}");
                throw new Exception($"Error retrieving order {orderId}: {ex.Message}", ex);
            }
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    return await context.GetOrdersByUserIdAsync(userId);
                }
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error retrieving orders for user {userId}");
                throw new Exception($"Error retrieving orders for user {userId}: {ex.Message}", ex);
            }
        }

        public async Task UpdateOrderStatusAsync(int orderId, string orderStatus, string paymentStatus = null, string trackingNumber = null, DateTime? shippedDate = null, DateTime? deliveredDate = null)
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    await context.UpdateOrderStatusAsync(orderId, orderStatus, paymentStatus, trackingNumber, shippedDate, deliveredDate);
                    _errorHandler.LogInfo($"Updated order {orderId} status to {orderStatus}");
                }
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error updating order {orderId} status");
                throw new Exception($"Error updating order {orderId} status: {ex.Message}", ex);
            }
        }

        public async Task<bool> OrderExistsAsync(int orderId)
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    var order = await context.GetOrderByIdAsync(orderId);
                    return order != null;
                }
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error checking if order {orderId} exists");
                throw new Exception($"Error checking if order {orderId} exists: {ex.Message}", ex);
            }
        }

        public async Task<bool> UserOwnsOrderAsync(int orderId, int userId)
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    var order = await context.GetOrderByIdAsync(orderId);
                    return order != null && order.UserId == userId;
                }
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error checking order {orderId} ownership for user {userId}");
                throw new Exception($"Error checking order {orderId} ownership for user {userId}: {ex.Message}", ex);
            }
        }
    }
} 