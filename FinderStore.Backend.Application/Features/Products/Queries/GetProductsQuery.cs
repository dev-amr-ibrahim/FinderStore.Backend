using AutoMapper;
using FinderStore.Backend.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using FinderStore.Backend.Application.DTOs;


namespace FinderStore.Backend.Application.Features.Products.Queries
{
    public record GetProductsQuery : IRequest<List<ProductDto>>
    {
        public Guid? CategoryId { get; init; }
        public bool? IsFeatured { get; init; }
        public string? SearchTerm { get; init; }
        public string? SortBy { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }

    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var query = _productRepository.Query()
                .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Where(p => p.IsActive);

            if (request.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == request.CategoryId.Value);

            if (request.IsFeatured.HasValue)
                query = query.Where(p => p.IsFeatured == request.IsFeatured.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    p.NameAr.Contains(term) ||
                    p.Description.ToLower().Contains(term) ||
                    p.Sku.ToLower().Contains(term));
            }

            query = request.SortBy?.ToLower() switch
            {
                "price-low" => query.OrderBy(p => p.Price),
                "price-high" => query.OrderByDescending(p => p.Price),
                "rating" => query.OrderByDescending(p => p.Rating),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };
            query = query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize);

            return await _mapper.ProjectTo<ProductDto>(query).ToListAsync(cancellationToken);
        }
    }
}