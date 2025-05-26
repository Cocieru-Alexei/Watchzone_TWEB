using System.Web;
using WatchZone.Domain.Model.Cart;
using WatchZone.Domain.Entities.User;

namespace WatchZone.BusinessLogic.Interface
{
    public interface ICartService
    {
        Cart GetCart(UserMinimal user, HttpSessionStateBase session);
        void SaveCart(UserMinimal user, Cart cart, HttpSessionStateBase session);
        void ClearCart(UserMinimal user, HttpSessionStateBase session);
    }
} 