using System;
using System.Linq;
using System.Web.Mvc;
using WatchZone.Web.Models;
using System.Threading.Tasks;

namespace WatchZone.Web.Controllers
{
    public class CartController : BaseController
    {
        private Cart GetCart()
        {
            try
            {
                // Use business logic method to get current user
                var user = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (user == null)
                {
                    ErrorHandler.LogWarning("Cart access attempted by unauthenticated user");
                    return null;
                }

                // Use business logic for user validation - check if user exists and is active
                if (!IsAuthenticated())
                {
                    ErrorHandler.LogWarning($"Cart access attempted with invalid authentication for user ID: {user.Id}");
                    return null;
                }

                string cartKey = $"Cart_{user.Id}";
                Cart cart = Session[cartKey] as Cart;
                if (cart == null)
                {
                    cart = new Cart();
                    Session[cartKey] = cart;
                    ErrorHandler.LogInfo($"New cart created for user {user.Username} (ID: {user.Id})");
                }
                return cart;
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "Error getting cart");
                return null;
            }
        }

        public ActionResult Index()
        {
            try
            {
                // Use BaseController authentication check
                var authResult = RedirectIfNotAuthenticated();
                if (authResult != null)
                    return authResult;

                var cart = GetCart();
                if (cart == null)
                {
                    return RedirectToAction("Login", "Auth");
                }
                return View(cart);
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Unable to load cart");
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddToCart(CartItem item)
        {
            try
            {
                // Use BaseController authentication check
                var authResult = RedirectIfNotAuthenticated();
                if (authResult != null)
                    return authResult;

                if (ModelState.IsValid)
                {
                    // Use business logic to validate user and get user info
                    var currentUser = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                    if (currentUser == null)
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    // Use business logic to validate product exists through ListingService
                    if (item.WatchId > 0)
                    {
                        var listing = await ListingService.GetListingByIdAsync(item.WatchId);
                        if (listing == null)
                        {
                            ErrorHandler.LogWarning($"User {currentUser.Username} (ID: {currentUser.Id}) attempted to add non-existent product to cart: Watch ID {item.WatchId}");
                            TempData["ErrorMessage"] = "The selected product no longer exists.";
                            return RedirectToAction("Index", "Home");
                        }

                        // Check if the item has already been sold
                        var isSold = await ListingService.IsListingSoldAsync(item.WatchId);
                        if (isSold)
                        {
                            ErrorHandler.LogWarning($"User {currentUser.Username} (ID: {currentUser.Id}) attempted to add sold product to cart: Watch ID {item.WatchId}");
                            TempData["ErrorMessage"] = "This item has already been sold and is no longer available.";
                            return RedirectToAction("Details", "Listings", new { id = item.WatchId });
                        }

                        // Use business logic to validate product details
                        if (listing.Price != item.Price)
                        {
                            ErrorHandler.LogWarning($"User {currentUser.Username} (ID: {currentUser.Id}) attempted to add product with incorrect price. Expected: {listing.Price:C}, Provided: {item.Price:C}");
                            TempData["ErrorMessage"] = "Product price has changed. Please refresh and try again.";
                            return RedirectToAction("Index", "Home");
                        }

                        // Update item details from business logic
                        item.WatchName = listing.Title;
                        item.Price = listing.Price;
                    }
                    
                    var cart = GetCart();
                    if (cart == null)
                    {
                        return RedirectToAction("Login", "Auth");
                    }
                    
                    cart.AddItem(item);
                    
                    // Use business logic for logging through ErrorHandler service
                    ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) added item to cart: {item.WatchName} - Quantity: {item.Quantity} - Price: {item.Price:C}");
                    
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "Error adding item to cart");
                TempData["ErrorMessage"] = "Failed to add item to cart. Please try again.";
            }
            
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult RemoveFromCart(int watchId)
        {
            try
            {
                // Use BaseController authentication check
                var authResult = RedirectIfNotAuthenticated();
                if (authResult != null)
                    return authResult;

                // Use business logic to validate user
                var currentUser = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser == null)
                {
                    ErrorHandler.LogWarning("Cart item removal attempted by unauthenticated user");
                    return RedirectToAction("Login", "Auth");
                }

                var cart = GetCart();
                if (cart == null)
                {
                    return RedirectToAction("Login", "Auth");
                }
                
                // Use business logic to validate removal operation
                var itemToRemove = cart.Items.FirstOrDefault(i => i.WatchId == watchId);
                if (itemToRemove == null)
                {
                    ErrorHandler.LogWarning($"User {currentUser.Username} (ID: {currentUser.Id}) attempted to remove non-existent item from cart: Watch ID {watchId}");
                    TempData["ErrorMessage"] = "Item not found in cart.";
                    return RedirectToAction("Index");
                }
                
                cart.RemoveItem(watchId);
                
                // Use business logic for comprehensive logging
                ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) removed item from cart: {itemToRemove.WatchName} - Watch ID {watchId}");
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, $"Error removing item from cart: Watch ID {watchId}");
                TempData["ErrorMessage"] = "Failed to remove item from cart. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int watchId, int quantity)
        {
            try
            {
                // Use BaseController authentication check
                var authResult = RedirectIfNotAuthenticated();
                if (authResult != null)
                    return authResult;

                // Use business logic to validate user
                var currentUser = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser == null)
                {
                    ErrorHandler.LogWarning("Cart quantity update attempted by unauthenticated user");
                    return RedirectToAction("Login", "Auth");
                }

                // Use business logic to validate quantity
                if (quantity <= 0)
                {
                    ErrorHandler.LogWarning($"User {currentUser.Username} (ID: {currentUser.Id}) attempted to set invalid quantity: {quantity} for Watch ID {watchId}");
                    TempData["ErrorMessage"] = "Quantity must be greater than 0.";
                    return RedirectToAction("Index");
                }

                var cart = GetCart();
                if (cart == null)
                {
                    return RedirectToAction("Login", "Auth");
                }
                
                // Use business logic to validate item exists before updating
                var itemToUpdate = cart.Items.FirstOrDefault(i => i.WatchId == watchId);
                if (itemToUpdate == null)
                {
                    ErrorHandler.LogWarning($"User {currentUser.Username} (ID: {currentUser.Id}) attempted to update quantity for non-existent item: Watch ID {watchId}");
                    TempData["ErrorMessage"] = "Item not found in cart.";
                    return RedirectToAction("Index");
                }

                var oldQuantity = itemToUpdate.Quantity;
                cart.UpdateQuantity(watchId, quantity);
                
                // Use business logic for comprehensive logging
                ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) updated cart quantity: {itemToUpdate.WatchName} - Watch ID {watchId}, Old Quantity: {oldQuantity}, New Quantity: {quantity}");
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, $"Error updating cart quantity: Watch ID {watchId}, Quantity {quantity}");
                TempData["ErrorMessage"] = "Failed to update cart quantity. Please try again.";
                return RedirectToAction("Index");
            }
        }

        public ActionResult ClearCart()
        {
            try
            {
                // Use BaseController authentication check
                var authResult = RedirectIfNotAuthenticated();
                if (authResult != null)
                    return authResult;

                // Use business logic to validate user
                var currentUser = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser == null)
                {
                    ErrorHandler.LogWarning("Cart clear attempted by unauthenticated user");
                    return RedirectToAction("Login", "Auth");
                }

                var cart = GetCart();
                if (cart == null)
                {
                    return RedirectToAction("Login", "Auth");
                }
                
                // Use business logic to validate operation and log details
                var itemCount = cart.Items.Count();
                var totalValue = cart.GetTotal();
                
                if (itemCount == 0)
                {
                    ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) attempted to clear already empty cart");
                    TempData["InfoMessage"] = "Cart is already empty.";
                    return RedirectToAction("Index");
                }
                
                cart.Clear();
                
                // Use business logic for comprehensive logging
                ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) cleared cart - {itemCount} items removed, Total value: {totalValue:C}");
                TempData["SuccessMessage"] = "Cart cleared successfully.";
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "Error clearing cart");
                TempData["ErrorMessage"] = "Failed to clear cart. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Checkout()
        {
            try
            {
                // Use BaseController authentication check
                var authResult = RedirectIfNotAuthenticated();
                if (authResult != null)
                    return authResult;

                // Use business logic to get current user information
                var currentUser = AuthService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser == null)
                {
                    ErrorHandler.LogWarning("Checkout attempted by unauthenticated user");
                    return RedirectToAction("Login", "Auth");
                }

                var cart = GetCart();
                if (cart == null || cart.Items.Count() == 0)
                {
                    ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) attempted checkout with empty cart");
                    TempData["ErrorMessage"] = "Your cart is empty.";
                    return RedirectToAction("Index");
                }

                // Use business logic for comprehensive logging
                var itemCount = cart.Items.Count();
                var totalAmount = cart.GetTotal();
                ErrorHandler.LogInfo($"User {currentUser.Username} (ID: {currentUser.Id}) initiated checkout with {itemCount} items, Total: {totalAmount:C}");

                // TODO: Implement checkout logic using injected services
                // This would involve:
                // 1. UserService for user validation
                // 2. ListingService for product validation
                // 3. OrderService for order creation (when implemented)
                // 4. PaymentService for payment processing (when implemented)
                
                // For now, just log the action and clear the cart
                cart.Clear();
                
                // Use business logic for success logging
                ErrorHandler.LogInfo($"Checkout completed successfully for user {currentUser.Username} (ID: {currentUser.Id})");
                TempData["SuccessMessage"] = "Checkout completed successfully!";
                
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "Error during checkout");
                TempData["ErrorMessage"] = "Checkout failed. Please try again.";
                return RedirectToAction("Index");
            }
        }
    }
} 