using AutoMapper;
using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Application.DTOs;
using MediatR;
using FinderStore.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinderStore.Backend.Application.Features.Orders.Queries;

public record GetUserOrdersQuery : IRequest<List<OrderSummaryDto>>
{
    public Guid UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, List<OrderSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUserOrdersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<OrderSummaryDto>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == request.UserId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Status = o.Status.ToString(),
                Total = o.Total,
                ItemCount = o.OrderItems.Count,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return orders;
    }
}