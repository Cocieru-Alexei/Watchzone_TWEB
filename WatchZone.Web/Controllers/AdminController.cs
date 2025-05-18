using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WatchZone.BusinessLogic;
using WatchZone.BusinessLogic.Interface.Repositories;
using WatchZone.Domain.Entities.User;
using WatchZone.Web.Models.Auth;
using WatchZone.BusinessLogic.Database;
using AutoMapper;

namespace WatchZone.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ISession _session;
        private readonly BussinesLogic _bl;

        public AdminController()
        {
            _bl = new BussinesLogic();
            _session = _bl.GetSessionBL();
        }

        public ActionResult Index()
        {
            // Get statistics for dashboard
            ViewBag.TotalUsers = GetTotalUsers();
            ViewBag.TotalProducts = GetTotalProducts();
            ViewBag.TotalOrders = GetTotalOrders();
            ViewBag.TotalRevenue = GetTotalRevenue();

            // Get data for tables
            ViewBag.Users = GetUsers();
            ViewBag.Products = GetProducts();
            ViewBag.Orders = GetOrders();

            return View("AdminPanel");
        }

        [HttpGet]
        public JsonResult GetUser(int id)
        {
            try
            {
                using (var db = new UserContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Id == id);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "User not found" }, JsonRequestBehavior.AllowGet);
                    }

                    var userMinimal = Mapper.Map<UserMinimal>(user);
                    return Json(new { success = true, data = userMinimal }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult AddUser(UserDataRegister userData)
        {
            try
            {
                var data = Mapper.Map<URegisterData>(userData);
                data.RegisterIp = Request.UserHostAddress;
                data.RegisterDateTime = DateTime.Now;

                var result = _session.UserRegister(data);
                return Json(new { success = result.Status, message = result.StatusMsg });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateUser(UserMinimal userData)
        {
            try
            {
                using (var db = new UserContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Id == userData.Id);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "User not found" });
                    }

                    user.Username = userData.Username;
                    user.Email = userData.Email;
                    user.Level = userData.Level;

                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { success = true, message = "User updated successfully" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteUser(int userId)
        {
            try
            {
                using (var db = new UserContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Id == userId);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "User not found" });
                    }

                    db.Users.Remove(user);
                    db.SaveChanges();

                    return Json(new { success = true, message = "User deleted successfully" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult AddProduct(ProductData productData)
        {
            try
            {
                // Implement product add logic
                return Json(new { success = true, message = "Product added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateProduct(ProductData productData)
        {
            try
            {
                // Implement product update logic
                return Json(new { success = true, message = "Product updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteProduct(int productId)
        {
            try
            {
                // Implement product delete logic
                return Json(new { success = true, message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                // Implement order status update logic
                return Json(new { success = true, message = "Order status updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #region Private Methods

        private int GetTotalUsers()
        {
            using (var db = new UserContext())
            {
                return db.Users.Count();
            }
        }

        private int GetTotalProducts()
        {
            // Implement get total products logic
            return 0;
        }

        private int GetTotalOrders()
        {
            // Implement get total orders logic
            return 0;
        }

        private decimal GetTotalRevenue()
        {
            // Implement get total revenue logic
            return 0;
        }

        private List<UserMinimal> GetUsers()
        {
            using (var db = new UserContext())
            {
                var users = db.Users.ToList();
                return Mapper.Map<List<UserMinimal>>(users);
            }
        }

        private List<ProductData> GetProducts()
        {
            // Implement get products logic
            return new List<ProductData>();
        }

        private List<OrderData> GetOrders()
        {
            // Implement get orders logic
            return new List<OrderData>();
        }

        #endregion
    }

    public class ProductData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
    }

    public class OrderData
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
    }
} 