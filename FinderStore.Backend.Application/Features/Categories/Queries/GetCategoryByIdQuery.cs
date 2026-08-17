using AutoMapper;
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

        public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdWithProductsAsync(request.Id, cancellationToken);

            return category == null ? null : _mapper.Map<CategoryDto>(category);
        }
    }
}
