using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchZone.Domain.Model.Basket;

namespace WatchZone.BusinessLogic.Interface
{
    public interface IBasketService
    {
        Task<decimal> CalculateTotalAsync(int userId);
        Task<int> AddItemAsync(int userId, int productId, int quantity);
        Task<bool> RemoveItemAsync(int userId, int productId);
        Task<bool> UpdateQuantityAsync(int userId, int productId, int quantity);
        Task<List<BasketItemDTO>> GetBasketItemsAsync(int userId);
        Task<bool> ClearBasketAsync(int userId);
    }
} 