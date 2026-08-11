using FinderStore.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinderStore.Backend.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<ApplicationUser> Users { get; }
        DbSet<Product> Products { get; }
        DbSet<Category> Categories { get; }
        DbSet<Order> Orders { get; }
        DbSet<OrderItem> OrderItems { get; }
        DbSet<OrderStatusHistory> OrderStatusHistories { get; }
        DbSet<ProductReview> ProductReviews { get; }
        DbSet<ProductImage> ProductImages { get; }
        DbSet<ProductVariant> ProductVariants { get; }
        DbSet<VariantOption> VariantOptions { get; }
        DbSet<ProductTag> ProductTags { get; }
        DbSet<Address> Addresses { get; }
        DbSet<WishlistItem> WishlistItems { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
