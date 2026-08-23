using AutoMapper;
using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Application.Constants;
using FinderStore.Backend.Application.DTOs;
using FinderStore.Backend.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinderStore.Backend.Application.Features.Categories.Queries
{
    public record GetCategoryByIdQuery : IRequest<CategoryDto?>
    {
        public Guid Id { get; init; }
    }

    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IBlobStorageService _blobStorageService;

        public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository, IMapper mapper, IBlobStorageService blobStorageService)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _blobStorageService = blobStorageService;
        }

        public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdWithProductsAsync(request.Id, cancellationToken);

            if (category is null)
                return null;

            var categoryDto = _mapper.Map<CategoryDto>(category);
            NormalizeImageUrls(categoryDto);
            return categoryDto;
        }

        private void NormalizeImageUrls(CategoryDto category)
        {
            category.ImageUrl = _blobStorageService.GetPublicUrl(category.ImageUrl, BlobContainers.Categories);
            foreach (var subCategory in category.SubCategories)
                NormalizeImageUrls(subCategory);
        }
    }
}
