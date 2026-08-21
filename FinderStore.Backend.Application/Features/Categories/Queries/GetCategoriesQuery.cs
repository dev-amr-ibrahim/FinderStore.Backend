using AutoMapper;
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

        public GetCategoriesQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
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

            return await _mapper.ProjectTo<CategoryDto>(query).ToListAsync(cancellationToken);
        }
    }

