using AutoMapper;
using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Application.Constants;
using FinderStore.Backend.Application.DTOs;
using FinderStore.Backend.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinderStore.Backend.Application.Features.Categories.Queries;

public record GetCategoriesQuery : IRequest<List<CategoryDto>>
{
    public bool? IncludeSubCategories { get; init; }
    public bool? OnlyActive { get; init; }
}

    public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IBlobStorageService _blobStorageService;

        public GetCategoriesQueryHandler(ICategoryRepository categoryRepository, IMapper mapper, IBlobStorageService blobStorageService)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _blobStorageService = blobStorageService;
        }

        public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var query = _categoryRepository.Query()
                .AsNoTracking()
                .Include(c => c.Products)
                .AsQueryable();

            if (request.OnlyActive == true)
                query = query.Where(c => c.IsActive);

            if (request.IncludeSubCategories == true)
                query = query.Include(c => c.SubCategories);

            query = query.OrderBy(c => c.DisplayOrder);

            var categories = await _mapper.ProjectTo<CategoryDto>(query).ToListAsync(cancellationToken);
            NormalizeImageUrls(categories);
            return categories;
        }

        private void NormalizeImageUrls(IEnumerable<CategoryDto> categories)
        {
            foreach (var category in categories)
            {
                category.ImageUrl = _blobStorageService.GetPublicUrl(category.ImageUrl, BlobContainers.Categories);
                NormalizeImageUrls(category.SubCategories);
            }
        }
    }

