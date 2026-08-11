using AutoMapper;
using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace FinderStore.Backend.Application.Features.Orders.Queries
{
    public record GetOrderByIdQuery : IRequest<OrderDto?>
    {
        public Guid OrderId { get; init; }
    }

    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetOrderByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .Include(o => o.StatusHistory.OrderByDescending(h => h.ChangedAt))
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

            return order == null ? null : _mapper.Map<OrderDto>(order);
        }
    }

}
