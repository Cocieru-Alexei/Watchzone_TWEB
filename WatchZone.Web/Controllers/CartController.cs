using System;
using System.Web.Mvc;
using WatchZone.Web.Models;
using WatchZone.BusinessLogic;
using WatchZone.Web.Filters;

namespace WatchZone.Web.Controllers
{
    [CustomAuthorize] // This ensures only logged-in users with a valid X-KEY cookie can access cart functionality
    public class CartController : Controller
    {
        private Cart GetCart()
        {
            var bl = new BussinesLogic();
            var session = bl.GetSessionBL();
            var user = session.GetUserByCookie(Request.Cookies["X-KEY"].Value);
            
            if (user == null)
            {
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

        public ActionResult Index()
        {
            var cart = GetCart();
            if (cart == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            return View(cart);
        }

        [HttpPost]
        public ActionResult AddToCart(CartItem item)
        {
            if (ModelState.IsValid)
            {
                var cart = GetCart();
                if (cart == null)
                {
                    return RedirectToAction("Login", "Auth");
                }
                cart.AddItem(item);
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult RemoveFromCart(int watchId)
        {
            var cart = GetCart();
            if (cart == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            cart.RemoveItem(watchId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int watchId, int quantity)
        {
            var cart = GetCart();
            if (cart == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            cart.UpdateQuantity(watchId, quantity);
            return RedirectToAction("Index");
        }

        public ActionResult ClearCart()
        {
            var cart = GetCart();
            if (cart == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            cart.Clear();
            return RedirectToAction("Index");
        }
    }
} 