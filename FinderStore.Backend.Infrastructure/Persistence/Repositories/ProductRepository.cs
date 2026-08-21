using FinderStore.Backend.Domain.Entities;
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

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(p => p.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(p => p.IsActive && p.IsFeatured).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(p => p.CategoryId == categoryId && p.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Options)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        return await _dbSet
            .Where(p => p.IsActive && (p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term) || p.Sku.ToLower().Contains(term)))
            .ToListAsync(cancellationToken);
    }
}
