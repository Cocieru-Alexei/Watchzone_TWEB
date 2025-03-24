using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchZone.BusinessLogic.Interface;
using WatchZone.BusinessLogic.Interface.Repositories;
using WatchZone.Domain.Model.Basket;

namespace WatchZone.BusinessLogic.BL_Struct.Basket
{
    public class BasketService : IBasketService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IProductRepository _productRepository;

        public BasketService(
            IBasketRepository basketRepository,
            IProductRepository productRepository)
        {
            _basketRepository = basketRepository;
            _productRepository = productRepository;
        }

        public async Task<decimal> CalculateTotalAsync(int userId)
        {
            var items = await _basketRepository.GetItemsByUserIdAsync(userId);
            decimal total = 0;

            foreach (var item in items)
            {
                total += item.UnitPrice * item.Quantity;
            }

            return total;
        }

        public async Task<int> AddItemAsync(int userId, int productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Cantitatea trebuie să fie mai mare decât 0.");

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException("Produsul nu există.");

            var existingItem = await _basketRepository.GetItemByUserIdAndProductIdAsync(userId, productId);
            
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.TotalPrice = existingItem.UnitPrice * existingItem.Quantity;
                await _basketRepository.UpdateAsync(existingItem);
                return existingItem.Id;
            }

            var newItem = new BasketItemDTO
            {
                UserId = userId,
                ProductId = productId,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity,
                TotalPrice = product.Price * quantity,
                AddedDate = DateTime.Now
            };

            return await _basketRepository.CreateAsync(newItem);
        }

        public async Task<bool> RemoveItemAsync(int userId, int productId)
        {
            var item = await _basketRepository.GetItemByUserIdAndProductIdAsync(userId, productId);
            if (item == null)
                return false;

            return await _basketRepository.DeleteAsync(item.Id);
        }

        public async Task<bool> UpdateQuantityAsync(int userId, int productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Cantitatea trebuie să fie mai mare decât 0.");

            var item = await _basketRepository.GetItemByUserIdAndProductIdAsync(userId, productId);
            if (item == null)
                return false;

            item.Quantity = quantity;
            item.TotalPrice = item.UnitPrice * quantity;
            await _basketRepository.UpdateAsync(item);
            return true;
        }

        public async Task<List<BasketItemDTO>> GetBasketItemsAsync(int userId)
        {
            return await _basketRepository.GetItemsByUserIdAsync(userId);
        }

        public async Task<bool> ClearBasketAsync(int userId)
        {
            return await _basketRepository.DeleteAllByUserIdAsync(userId);
        }
    }
} 