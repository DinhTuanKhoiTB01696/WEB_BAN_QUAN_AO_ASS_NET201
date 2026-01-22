using Assignment_NET201.Data;
using Assignment_NET201.Extensions;
using Assignment_NET201.Models;
using Assignment_NET201.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment_NET201.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        public IActionResult AddToCart(int productId, int quantity = 1, string size = "M")
        {
            var product = _context.Products.Find(productId);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId && c.Size == size);

            int currentInCart = existingItem?.Quantity ?? 0;
            int totalNewQuantity = currentInCart + quantity;

            if (totalNewQuantity > product.Quantity)
            {
                TempData["Message"] = $"Số lượng chỉ còn {product.Quantity} không thể thêm";
                totalNewQuantity = product.Quantity;
            }

            if (totalNewQuantity <= 0 && product.Quantity > 0)
            {
                // This shouldn't happen with quantity = 1, but for safety
                totalNewQuantity = 1; 
            }
            
            if (product.Quantity <= 0)
            {
                TempData["Message"] = "Sản phẩm đã hết hàng";
                return RedirectToAction("Index");
            }

            if (existingItem != null)
            {
                existingItem.Quantity = totalNewQuantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = totalNewQuantity,
                    ImageUrl = product.ImageUrl,
                    Size = size
                });
            }

            HttpContext.Session.Set("Cart", cart);
            return RedirectToAction("Index");
        }

        public IActionResult RemoveFromCart(int productId, string size)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == productId && c.Size == size);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.Set("Cart", cart);
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (!cart.Any()) return RedirectToAction("Index");

            var user = await _userManager.GetUserAsync(User);
            ViewBag.CurrentUser = user;

            return View(cart);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (!cart.Any()) return RedirectToAction("Index");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalAmount = cart.Sum(c => c.Total),
                OrderDetails = cart.Select(c => new OrderDetail
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    Price = c.Price
                }).ToList()
            };

            // Decrease product quantities
            foreach (var item in cart)
            {
                var p = await _context.Products.FindAsync(item.ProductId);
                if (p != null)
                {
                    p.Quantity -= item.Quantity;
                    if (p.Quantity < 0) p.Quantity = 0;
                    _context.Update(p);
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Clear Cart
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        [Authorize]
        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
    }
}
