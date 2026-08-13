using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinderStore.Backend.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

        public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

        public DbSet<VariantOption> VariantOptions => Set<VariantOption>();

        public DbSet<ProductTag> ProductTags => Set<ProductTag>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>(order =>
            {
                order.OwnsOne(x => x.ShippingAddress);
                order.OwnsOne(x => x.BillingAddress);
                order.OwnsOne(x => x.PaymentMethod);
            });

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ApplicationDbContext).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }

    public class ApplicationRole : IdentityRole<Guid>
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName) { }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
