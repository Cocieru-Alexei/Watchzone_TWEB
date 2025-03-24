using System.Collections.Generic;
using System.Threading.Tasks;
using WatchZone.Domain.Model.Basket;

namespace WatchZone.BusinessLogic.Interface.Repositories
{
    public interface IBasketRepository
    {
        Task<List<BasketItemDTO>> GetItemsByUserIdAsync(int userId);
        Task<BasketItemDTO> GetItemByUserIdAndProductIdAsync(int userId, int productId);
        Task<int> CreateAsync(BasketItemDTO item);
        Task<bool> UpdateAsync(BasketItemDTO item);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteAllByUserIdAsync(int userId);
    }
} 