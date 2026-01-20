using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Assignment_NET201.Models;
using Assignment_NET201.Data;
using Microsoft.EntityFrameworkCore;

namespace Assignment_NET201.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Fetch products for "Best Sellers" (Simulated by taking top 8)
        var products = await _context.Products.Take(8).ToListAsync();
        return View(products);
    }

    public async Task<IActionResult> Shop(string searchString, decimal? minPrice, decimal? maxPrice, int? categoryId)
    {
        var products = _context.Products.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            products = products.Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString));
        }

        if (minPrice.HasValue)
        {
            products = products.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            products = products.Where(p => p.Price <= maxPrice.Value);
        }

        if (categoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == categoryId.Value);
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(await products.ToListAsync());
    }

    public async Task<IActionResult> ProductDetail(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
