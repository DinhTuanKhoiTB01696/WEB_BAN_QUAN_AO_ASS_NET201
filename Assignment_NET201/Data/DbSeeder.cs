using Assignment_NET201.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment_NET201.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed Roles
            string[] roles = { "Admin", "Customer", "Guest" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Admin User
            var adminEmail = "admin@cloth.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin",
                    Address = "HQ",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Categories
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(new List<Category>
                {
                    new Category { Name = "T-Shirts" },
                    new Category { Name = "Jeans" },
                    new Category { Name = "Jackets" },
                    new Category { Name = "Accessories" }
                });
                await context.SaveChangesAsync();
            }

            // Seed Products (10 items)
            if (!context.Products.Any())
            {
                var tshirts = context.Categories.First(c => c.Name == "T-Shirts");
                var jeans = context.Categories.First(c => c.Name == "Jeans");
                var jackets = context.Categories.First(c => c.Name == "Jackets");

                context.Products.AddRange(new List<Product>
                {
                    // T-Shirts
                    new Product { Name = "Basic White Tee", Description = "Premium cotton essential.", Price = 250000, CategoryId = tshirts.Id, ImageUrl = "/images/man-basic-white-polo-shirt-apparel-studio-shoot.jpg" },
                    new Product { Name = "Graphic Print Tee", Description = "Urban style graphic tee.", Price = 350000, CategoryId = tshirts.Id, ImageUrl = "https://placehold.co/400x500?text=Graphic+Tee" },
                    new Product { Name = "Oversized Black Tee", Description = "Streetwear fit.", Price = 300000, CategoryId = tshirts.Id, ImageUrl = "https://placehold.co/400x500?text=Black+Tee" },
                    
                    // Jeans
                    new Product { Name = "Slim Fit Ripped Jeans", Description = "Edgy look with comfort stretch.", Price = 650000, CategoryId = jeans.Id, ImageUrl = "https://placehold.co/400x500?text=Ripped+Jeans" },
                    new Product { Name = "Classic Straight Cut", Description = "Timeless denim style.", Price = 600000, CategoryId = jeans.Id, ImageUrl = "https://placehold.co/400x500?text=Classic+Jeans" },
                    new Product { Name = "Black Skinny Jeans", Description = "Rockstar aesthetic.", Price = 700000, CategoryId = jeans.Id, ImageUrl = "https://placehold.co/400x500?text=Skinny+Jeans" },

                    // Jackets
                    new Product { Name = "Denim Jacket", Description = "Rugged and durable.", Price = 900000, CategoryId = jackets.Id, ImageUrl = "https://placehold.co/400x500?text=Denim+Jacket" },
                    new Product { Name = "Bomber Jacket", Description = "Classic aviator style.", Price = 850000, CategoryId = jackets.Id, ImageUrl = "https://placehold.co/400x500?text=Bomber" },
                    
                    // More T-Shirts
                    new Product { Name = "Striped Polo", Description = "Smart casual choice.", Price = 400000, CategoryId = tshirts.Id, ImageUrl = "/images/man-woman-basic-white-polo-shirts-apparel-studio-shoot.jpg" }, // Using the second image I saw earlier
                    new Product { Name = "V-Neck Sweater", Description = "Lightweight knit.", Price = 450000, CategoryId = tshirts.Id, ImageUrl = "https://placehold.co/400x500?text=Sweater" }
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
