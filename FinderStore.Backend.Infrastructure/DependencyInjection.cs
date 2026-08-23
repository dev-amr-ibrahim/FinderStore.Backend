using Azure.Identity;
using Azure.Storage.Blobs;
using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Domain.Common.Interfaces;
using FinderStore.Backend.Domain.Entities;
using FinderStore.Backend.Domain.Repositories;
using FinderStore.Backend.Infrastructure.Data;
using FinderStore.Backend.Infrastructure.Persistence;
using FinderStore.Backend.Infrastructure.Persistence.Repositories;
using FinderStore.Backend.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinderStore.Backend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database Context
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                }));

        // ============================================
        // Repositories
        // ============================================
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
        services.AddScoped<IProductImageRepository, ProductImageRepository>();
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        services.AddScoped<IVariantOptionRepository, VariantOptionRepository>();
        services.AddScoped<IProductTagRepository, ProductTagRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IWishlistItemRepository, WishlistItemRepository>();
        services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();

        // ============================================
        // Identity Configuration
        // ============================================
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

            // Sign-in settings
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedAccount = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddSignInManager();

        // Configure Application Cookie
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.ExpireTimeSpan = TimeSpan.FromHours(2);
            options.SlidingExpiration = true;
        });

        // ============================================
        // Register Services
        // ============================================

        // Token Service
        services.AddScoped<ITokenService, TokenService>();

        // Cache Service
        services.AddMemoryCache();
        services.AddDistributedMemoryCache();

        // HTTP Context Accessor
        services.AddHttpContextAccessor();


        services.AddSingleton<BlobServiceClient>(sp =>
        {
            var accountName =
                configuration["AzureStorage:AccountName"]
                ?? throw new InvalidOperationException(
                    "AzureStorage:AccountName is missing.");

            var credential = new DefaultAzureCredential();

            return new BlobServiceClient(
                new Uri(
                    $"https://{accountName}.blob.core.windows.net"),
                credential);
        });

        services.AddScoped<IBlobStorageService, BlobStorageService>();

        return services;
    }
}