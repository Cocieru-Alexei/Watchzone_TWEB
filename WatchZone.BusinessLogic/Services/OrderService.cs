using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchZone.Domain.Database;
using WatchZone.Domain.Model;

namespace WatchZone.BusinessLogic.Services
{
    public class OrderService
    {
        private readonly DatabaseContext _databaseContext;

        public OrderService()
        {
            _databaseContext = new DatabaseContext();
        }

        public async Task<int> CreateOrderAsync(Order order)
        {
            try
            {
                return await _databaseContext.CreateOrderAsync(order);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating order: {ex.Message}", ex);
            }
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            try
            {
                return await _databaseContext.GetOrderByIdAsync(orderId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving order {orderId}: {ex.Message}", ex);
            }
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
            try
            {
                return await _databaseContext.GetOrdersByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving orders for user {userId}: {ex.Message}", ex);
            }
        }

        public async Task UpdateOrderStatusAsync(int orderId, string orderStatus, string paymentStatus = null, string trackingNumber = null, DateTime? shippedDate = null, DateTime? deliveredDate = null)
        {
            try
            {
                await _databaseContext.UpdateOrderStatusAsync(orderId, orderStatus, paymentStatus, trackingNumber, shippedDate, deliveredDate);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating order {orderId} status: {ex.Message}", ex);
            }
        }

        public async Task<bool> OrderExistsAsync(int orderId)
        {
            try
            {
                var order = await _databaseContext.GetOrderByIdAsync(orderId);
                return order != null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking if order {orderId} exists: {ex.Message}", ex);
            }
        }

        public async Task<bool> UserOwnsOrderAsync(int orderId, int userId)
        {
            try
            {
                var order = await _databaseContext.GetOrderByIdAsync(orderId);
                return order != null && order.UserId == userId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking order {orderId} ownership for user {userId}: {ex.Message}", ex);
            }
        }
    }
} 