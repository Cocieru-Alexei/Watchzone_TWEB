using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WatchZone.Web.Models;
using WatchZone.Domain.Model;

namespace WatchZone.Web.Controllers
{
    public class OrderController : BaseController
    {
        private Cart GetCart()
        {
            try
            {
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var authService = businessLogic.GetAuthService();
                var errorHandler = businessLogic.GetErrorHandler();

                var user = authService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (user == null)
                {
                    errorHandler.LogWarning("Cart access attempted by unauthenticated user");
                    return null;
                }

                if (!IsAuthenticated())
                {
                    errorHandler.LogWarning($"Cart access attempted with invalid authentication for user ID: {user.Id}");
                    return null;
                }

                string cartKey = $"Cart_{user.Id}";
                Cart cart = Session[cartKey] as Cart;
                if (cart == null)
                {
                    cart = new Cart();
                    Session[cartKey] = cart;
                }
                return cart;
            }
            catch (Exception ex)
            {
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var errorHandler = businessLogic.GetErrorHandler();
                errorHandler.LogError(ex, "Error getting cart");
                return null;
            }
        }

        public ActionResult Checkout()
        {
            try
            {
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var authService = businessLogic.GetAuthService();
                var errorHandler = businessLogic.GetErrorHandler();

                var authResult = RedirectIfNotAuthenticated();
                if (authResult != null)
                    return authResult;

                var cart = GetCart();
                if (cart == null || !cart.Items.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty.";
                    return RedirectToAction("Index", "Cart");
                }

                var viewModel = new CheckoutViewModel
                {
                    Cart = cart,
                    TotalAmount = cart.GetTotal()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Unable to load checkout page");
            }
        }

        [HttpPost]
        public async Task<ActionResult> ProcessCheckout(CheckoutViewModel model)
        {
            try
            {
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var authService = businessLogic.GetAuthService();
                var errorHandler = businessLogic.GetErrorHandler();

                var authResult = RedirectIfNotAuthenticated();
                if (authResult != null)
                    return authResult;

                var currentUser = authService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var cart = GetCart();
                if (cart == null || !cart.Items.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty.";
                    return RedirectToAction("Index", "Cart");
                }

                if (!ModelState.IsValid)
                {
                    model.Cart = cart;
                    model.TotalAmount = cart.GetTotal();
                    return View("Checkout", model);
                }

                // Create order
                var order = new Order
                {
                    UserId = currentUser.Id,
                    OrderDate = DateTime.Now,
                    TotalAmount = cart.GetTotal(),
                    OrderStatus = "Ordered",
                    PaymentStatus = "Paid",
                    ShippingAddress = model.ShippingAddress,
                    BillingAddress = model.BillingAddress,
                    PaymentMethod = model.PaymentMethod,
                    Notes = model.Notes
                };

                // Add order items
                foreach (var cartItem in cart.Items)
                {
                    var orderItem = new OrderItem
                    {
                        ListingId = cartItem.WatchId,
                        ProductName = cartItem.WatchName,
                        UnitPrice = cartItem.Price,
                        Quantity = cartItem.Quantity,
                        TotalPrice = cartItem.Price * cartItem.Quantity,
                        ProductImageUrl = cartItem.ImageUrl
                    };
                    order.OrderItems.Add(orderItem);
                }

                // Save order to database using business logic
                var orderService = businessLogic.GetOrderService();
                var orderId = await orderService.CreateOrderAsync(order);
                
                // Clear cart after successful order
                cart.Clear();

                errorHandler.LogInfo($"Order {orderId} created successfully for user {currentUser.Username} (ID: {currentUser.Id}), Total: {order.TotalAmount:C}");
                
                TempData["SuccessMessage"] = $"Order #{orderId} has been placed successfully!";
                return RedirectToAction("OrderConfirmation", new { orderId = orderId });
            }
            catch (Exception ex)
            {
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var errorHandler = businessLogic.GetErrorHandler();
                errorHandler.LogError(ex, "Error processing checkout");
                TempData["ErrorMessage"] = "There was an error processing your order. Please try again.";
                
                model.Cart = GetCart();
                model.TotalAmount = model.Cart?.GetTotal() ?? 0;
                return View("Checkout", model);
            }
        }

        public async Task<ActionResult> OrderConfirmation(int orderId)
        {
            try
            {
                var authResult = RedirectIfNotAuthenticated();
                if (authResult != null)
                    return authResult;

                // Get order from database
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var orderService = businessLogic.GetOrderService();
                var authService = businessLogic.GetAuthService();
                
                var currentUser = authService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var order = await orderService.GetOrderByIdAsync(orderId);
                if (order == null || order.UserId != currentUser.Id)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("Index", "Home");
                }

                var viewModel = new OrderViewModel
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    OrderStatus = order.OrderStatus,
                    PaymentStatus = order.PaymentStatus,
                    ShippingAddress = order.ShippingAddress,
                    BillingAddress = order.BillingAddress,
                    PaymentMethod = order.PaymentMethod,
                    Notes = order.Notes,
                    OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        OrderItemId = oi.OrderItemId,
                        ListingId = oi.ListingId,
                        ProductName = oi.ProductName,
                        UnitPrice = oi.UnitPrice,
                        Quantity = oi.Quantity,
                        TotalPrice = oi.TotalPrice,
                        ProductImageUrl = oi.ProductImageUrl
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Unable to load order confirmation");
            }
        }

        public async Task<ActionResult> MyOrders()
        {
            try
            {
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var authService = businessLogic.GetAuthService();
                var orderService = businessLogic.GetOrderService();

                var authResult = RedirectIfNotAuthenticated();
                if (authResult != null)
                    return authResult;

                var currentUser = authService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Get orders from database
                var dbOrders = await orderService.GetOrdersByUserIdAsync(currentUser.Id);
                var orders = dbOrders.Select(order => new OrderViewModel
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    OrderStatus = order.OrderStatus,
                    PaymentStatus = order.PaymentStatus,
                    PaymentMethod = order.PaymentMethod,
                    OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        OrderItemId = oi.OrderItemId,
                        ListingId = oi.ListingId,
                        ProductName = oi.ProductName,
                        UnitPrice = oi.UnitPrice,
                        Quantity = oi.Quantity,
                        TotalPrice = oi.TotalPrice,
                        ProductImageUrl = oi.ProductImageUrl
                    }).ToList()
                }).ToList();
                return View(orders);
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Unable to load orders");
            }
        }

        public async Task<ActionResult> OrderDetails(int orderId)
        {
            try
            {
                var businessLogic = new WatchZone.BusinessLogic.BussinesLogic();
                var authService = businessLogic.GetAuthService();
                var orderService = businessLogic.GetOrderService();

                var authResult = RedirectIfNotAuthenticated();
                if (authResult != null)
                    return authResult;

                var currentUser = authService.GetUserByCookie(Request.Cookies["X-KEY"]?.Value);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Get order from database
                var order = await orderService.GetOrderByIdAsync(orderId);
                if (order == null || order.UserId != currentUser.Id)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("MyOrders");
                }

                var viewModel = new OrderViewModel
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    OrderStatus = order.OrderStatus,
                    PaymentStatus = order.PaymentStatus,
                    ShippingAddress = order.ShippingAddress,
                    BillingAddress = order.BillingAddress,
                    PaymentMethod = order.PaymentMethod,
                    TrackingNumber = order.TrackingNumber,
                    ShippedDate = order.ShippedDate,
                    DeliveredDate = order.DeliveredDate,
                    Notes = order.Notes,
                    OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        OrderItemId = oi.OrderItemId,
                        ListingId = oi.ListingId,
                        ProductName = oi.ProductName,
                        UnitPrice = oi.UnitPrice,
                        Quantity = oi.Quantity,
                        TotalPrice = oi.TotalPrice,
                        ProductImageUrl = oi.ProductImageUrl
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Unable to load order details");
            }
        }
    }
} 