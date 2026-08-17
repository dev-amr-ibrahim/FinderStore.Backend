using AutoMapper;
using FinderStore.Backend.Application.DTOs;
using FinderStore.Backend.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinderStore.Backend.Application.Features.Products.Queries;

public record GetProductReviewsQuery : IRequest<List<ReviewDto>>
{
    public Guid ProductId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SortBy { get; init; } // "newest", "highest", "lowest", "helpful"
}

public class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, List<ReviewDto>>
{
    private readonly IProductReviewRepository _productReviewRepository;
    private readonly IMapper _mapper;

    public GetProductReviewsQueryHandler(IProductReviewRepository productReviewRepository, IMapper mapper)
    {
        _productReviewRepository = productReviewRepository;
        _mapper = mapper;
    }

    public async Task<List<ReviewDto>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var query = _productReviewRepository.Query()
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.ProductId == request.ProductId && r.IsApproved);

        query = request.SortBy?.ToLower() switch
        {
            "highest" => query.OrderByDescending(r => r.Rating),
            "lowest" => query.OrderBy(r => r.Rating),
            "helpful" => query.OrderByDescending(r => r.HelpfulCount),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };

        query = query.Skip((request.Page - 1) * request.PageSize)
                     .Take(request.PageSize);

        return await _mapper.ProjectTo<ReviewDto>(query).ToListAsync(cancellationToken);
    }
}