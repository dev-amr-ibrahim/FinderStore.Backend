using FinderStore.Backend.Domain.Common.Interfaces;
using FinderStore.Backend.Domain.Entities;
using FinderStore.Backend.Domain.Enums;
using FinderStore.Backend.Domain.Repositories;
using FinderStore.Backend.Infrastructure.Data;
using FinderStore.Backend.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FinderStore.Backend.Infrastructure.Persistence.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => c.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdWithProductsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Products)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Order>> GetOrdersByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(o => o.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.Status == OrderStatus.Delivered)
            .SumAsync(o => o.Total, cancellationToken);
    }

    public async Task<decimal> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.Status == OrderStatus.Delivered)
            .SumAsync(o => o.Total, cancellationToken);
    }

    public async Task<int> GetPendingOrdersCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);
    }
}

public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
{
    public OrderItemRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<OrderItem>> GetItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(oi => oi.OrderId == orderId).ToListAsync(cancellationToken);
    }
}

public class ProductReviewRepository : Repository<ProductReview>, IProductReviewRepository
{
    public ProductReviewRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<ProductReview>> GetReviewsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.ProductId == productId && r.IsApproved)
            .Include(r => r.User)
            .ToListAsync(cancellationToken);
    }
}

public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
{
    public ProductImageRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<ProductImage>> GetImagesByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(i => i.ProductId == productId).ToListAsync(cancellationToken);
    }
}

public class ProductVariantRepository : Repository<ProductVariant>, IProductVariantRepository
{
    public ProductVariantRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<ProductVariant>> GetVariantsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(v => v.ProductId == productId).ToListAsync(cancellationToken);
    }
}

public class VariantOptionRepository : Repository<VariantOption>, IVariantOptionRepository
{
    public VariantOptionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<VariantOption>> GetOptionsByVariantIdAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(o => o.ProductVariantId == variantId).ToListAsync(cancellationToken);
    }
}

public class ProductTagRepository : Repository<ProductTag>, IProductTagRepository
{
    public ProductTagRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<ProductTag>> GetTagsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(t => t.ProductId == productId).ToListAsync(cancellationToken);
    }
}

public class AddressRepository : Repository<Address>, IAddressRepository
{
    public AddressRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Address>> GetAddressesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(a => a.UserId == userId).ToListAsync(cancellationToken);
    }
}

public class WishlistItemRepository : Repository<WishlistItem>, IWishlistItemRepository
{
    public WishlistItemRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<WishlistItem>> GetWishlistByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(w => w.UserId == userId).ToListAsync(cancellationToken);
    }
}

public class OrderStatusHistoryRepository : Repository<OrderStatusHistory>, IOrderStatusHistoryRepository
{
    public OrderStatusHistoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<OrderStatusHistory>> GetHistoryByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(h => h.OrderId == orderId).ToListAsync(cancellationToken);
    }
}
