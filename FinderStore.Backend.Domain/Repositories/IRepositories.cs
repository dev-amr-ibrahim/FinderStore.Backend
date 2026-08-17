using FinderStore.Backend.Domain.Entities;
using FinderStore.Backend.Domain.Common.Interfaces;

namespace FinderStore.Backend.Domain.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetByIdWithProductsAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IOrderRepository : IRepository<Order>
{
    Task<IReadOnlyList<Order>> GetOrdersByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<int> GetPendingOrdersCountAsync(CancellationToken cancellationToken = default);
}

public interface IOrderItemRepository : IRepository<OrderItem>
{
    Task<IReadOnlyList<OrderItem>> GetItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public interface IProductReviewRepository : IRepository<ProductReview>
{
    Task<IReadOnlyList<ProductReview>> GetReviewsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
}

public interface IProductImageRepository : IRepository<ProductImage>
{
    Task<IReadOnlyList<ProductImage>> GetImagesByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
}

public interface IProductVariantRepository : IRepository<ProductVariant>
{
    Task<IReadOnlyList<ProductVariant>> GetVariantsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
}

public interface IVariantOptionRepository : IRepository<VariantOption>
{
    Task<IReadOnlyList<VariantOption>> GetOptionsByVariantIdAsync(Guid variantId, CancellationToken cancellationToken = default);
}

public interface IProductTagRepository : IRepository<ProductTag>
{
    Task<IReadOnlyList<ProductTag>> GetTagsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
}

public interface IAddressRepository : IRepository<Address>
{
    Task<IReadOnlyList<Address>> GetAddressesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IWishlistItemRepository : IRepository<WishlistItem>
{
    Task<IReadOnlyList<WishlistItem>> GetWishlistByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IOrderStatusHistoryRepository : IRepository<OrderStatusHistory>
{
    Task<IReadOnlyList<OrderStatusHistory>> GetHistoryByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}
