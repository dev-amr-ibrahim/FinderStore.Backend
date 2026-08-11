using FinderStore.Backend.Domain.Entities;
using FinderStore.Backend.Domain.Enums;
using FinderStore.Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace FinderStore.Infrastructure.Data.Context;

public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed Roles
        foreach (var role in UserRoles.AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role,
                    Description = $"{role} role"
                });
            }
        }

        // Seed Admin User
        var adminEmail = "admin@finderstore.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Admin",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                AvatarUrl = "https://ui-avatars.com/api/?name=System+Admin&background=f9a726&color=fff"
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123456");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                await userManager.AddToRoleAsync(adminUser, UserRoles.Manager);
            }
        }

        // Seed Categories
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                Category.Create(
                    "Electronics", "إلكترونيات", "electronics",
                    "Latest gadgets and tech devices", "أحدث الأجهزة والإلكترونيات",
                    "https://images.unsplash.com/photo-1498049794561-7780e7231661?w=400",
                    1, null),
                Category.Create(
                    "Fashion", "أزياء", "fashion",
                    "Trendy clothing and accessories", "ملابس وإكسسوارات عصرية",
                    "https://images.unsplash.com/photo-1445205170230-053b83016050?w=400",
                    2, null),
                Category.Create(
                    "Home & Living", "المنزل والمعيشة", "home-living",
                    "Beautiful home decor and furniture", "ديكور وأثاث منزلي جميل",
                    "https://images.unsplash.com/photo-1484101403633-562f891dc89a?w=400",
                    3, null),
                Category.Create(
                    "Beauty", "الجمال", "beauty",
                    "Premium beauty and skincare products", "منتجات التجميل والعناية بالبشرة",
                    "https://images.unsplash.com/photo-1522335789203-aabd1fc54bc9?w=400",
                    4, null),
                Category.Create(
                    "Sports", "الرياضة", "sports",
                    "Sports equipment and activewear", "معدات وملابس رياضية",
                    "https://images.unsplash.com/photo-1461896836934-bd45ba220cf4?w=400",
                    5, null),
                Category.Create(
                    "Car Accessories", "carAccessories", "Car Accessories",
                    "Best-selling car accessories", "أفضل اكسسوارات السيارات",
                    "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?w=400",
                    6, null)
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // Seed Sample Products
        if (!context.Products.Any())
        {
            var electronicsCategory = context.Categories.First(c => c.Slug == "electronics");
            var fashionCategory = context.Categories.First(c => c.Slug == "fashion");

            var products = new List<Product>
            {
                Product.Create(
                    "Premium Wireless Headphones",
                    "سماعات لاسلكية فاخرة",
                    "Experience crystal-clear audio with premium noise cancellation technology.",
                    "استمتع بصوت نقي مع تقنية إلغاء الضوضاء الفاخرة.",
                    299.99m, 349.99m, "WH-001", 50,
                    electronicsCategory.Id, adminEmail),

                Product.Create(
                    "Minimalist Watch",
                    "ساعة مينيمالية",
                    "Elegant minimalist watch with genuine leather strap.",
                    "ساعة أنيقة بتصميم مينيمالي مع حزام جلد طبيعي.",
                    199.99m, 249.99m, "MW-002", 30,
                    fashionCategory.Id, adminEmail)
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}