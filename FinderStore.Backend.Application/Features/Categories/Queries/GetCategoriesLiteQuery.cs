using AutoMapper;
using FinderStore.Backend.Application.DTOs;
using FinderStore.Backend.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinderStore.Backend.Application.Features.Categories.Queries;

public record GetCategoriesLiteQuery : IRequest<List<CategoryLiteDto>>
{
    public bool? OnlyActive { get; init; }
}

    public class GetCategoriesLiteQueryQueryHandler : IRequestHandler<GetCategoriesLiteQuery, List<CategoryLiteDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public GetCategoriesLiteQueryQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<List<CategoryLiteDto>> Handle(GetCategoriesLiteQuery request, CancellationToken cancellationToken)
        {
            var query = _categoryRepository.Query()
                .AsNoTracking()
                .Include(c => c.Products)
                .AsQueryable();

            if (request.OnlyActive == true)
                query = query.Where(c => c.IsActive);


            query = query.OrderBy(c => c.DisplayOrder);

            return await _mapper.ProjectTo<CategoryLiteDto>(query).ToListAsync(cancellationToken);
        }
    }

