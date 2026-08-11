using AutoMapper;
using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Application.DTOs;
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
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCategoriesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Categories
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

