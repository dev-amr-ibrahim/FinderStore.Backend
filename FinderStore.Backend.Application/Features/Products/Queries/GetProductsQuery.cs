using AutoMapper;
using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Application.Constants;
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
        public bool IncludeInactive { get; init; }
        public bool? IsActive { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }

    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly IBlobStorageService _blobStorageService;

        public GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper, IBlobStorageService blobStorageService)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _blobStorageService = blobStorageService;
        }

        public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<FinderStore.Backend.Domain.Entities.Product> query = _productRepository.Query()
                .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            ;

            if (!request.IncludeInactive)
                query = query.Where(p => p.IsActive && p.Category.IsActive);

            if (request.IncludeInactive && request.IsActive.HasValue)
                query = query.Where(p => p.IsActive == request.IsActive.Value);
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

            var products = await _mapper.ProjectTo<ProductDto>(query).ToListAsync(cancellationToken);

            foreach (var product in products)
            {
                foreach (var image in product.Images)
                    image.Url = _blobStorageService.GetPublicUrl(image.Url, BlobContainers.Products);

                if (product.Category is not null)
                    product.Category.ImageUrl = _blobStorageService.GetPublicUrl(product.Category.ImageUrl, BlobContainers.Categories);
            }

            return products;
        }
    }
}