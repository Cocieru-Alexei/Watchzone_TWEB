using System.Threading.Tasks;
using WatchZone.Domain.Model.Product;

namespace WatchZone.BusinessLogic.Interface.Repositories
{
    public interface IProductRepository
    {
        Task<ProductDTO> GetByIdAsync(int id);
    }
} 