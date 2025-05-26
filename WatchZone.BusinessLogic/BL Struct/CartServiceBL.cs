using System;
using System.Web;
using WatchZone.BusinessLogic.Interface;
using WatchZone.Domain.Model.Cart;
using WatchZone.Domain.Entities.User;

namespace WatchZone.BusinessLogic.BL_Struct
{
    public class CartServiceBL : ICartService
    {
        private readonly IErrorHandler _errorHandler;

        public CartServiceBL(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public Cart GetCart(UserMinimal user, HttpSessionStateBase session)
        {
            try
            {
                if (user == null)
                {
                    _errorHandler.LogWarning("Cart access attempted with null user");
                    return null;
                }

                string cartKey = $"Cart_{user.Id}";
                Cart cart = session[cartKey] as Cart;
                
                if (cart == null)
                {
                    cart = new Cart();
                    session[cartKey] = cart;
                    _errorHandler.LogInfo($"New cart created for user {user.Username} (ID: {user.Id})");
                }
                
                return cart;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error getting cart for user {user?.Username} (ID: {user?.Id})");
                return null;
            }
        }

        public void SaveCart(UserMinimal user, Cart cart, HttpSessionStateBase session)
        {
            try
            {
                if (user == null || cart == null)
                {
                    _errorHandler.LogWarning("Cart save attempted with null user or cart");
                    return;
                }

                string cartKey = $"Cart_{user.Id}";
                session[cartKey] = cart;
                _errorHandler.LogInfo($"Cart saved for user {user.Username} (ID: {user.Id})");
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error saving cart for user {user?.Username} (ID: {user?.Id})");
            }
        }

        public void ClearCart(UserMinimal user, HttpSessionStateBase session)
        {
            try
            {
                if (user == null)
                {
                    _errorHandler.LogWarning("Cart clear attempted with null user");
                    return;
                }

                string cartKey = $"Cart_{user.Id}";
                session.Remove(cartKey);
                _errorHandler.LogInfo($"Cart cleared for user {user.Username} (ID: {user.Id})");
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"Error clearing cart for user {user?.Username} (ID: {user?.Id})");
            }
        }
    }
} 