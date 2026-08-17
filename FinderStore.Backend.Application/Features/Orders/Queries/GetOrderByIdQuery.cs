using AutoMapper;
using FinderStore.Backend.Application.DTOs;
using FinderStore.Backend.Domain.Repositories;
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
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public GetOrderByIdQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.Query()
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .Include(o => o.StatusHistory.OrderByDescending(h => h.ChangedAt))
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

            return order == null ? null : _mapper.Map<OrderDto>(order);
        }
    }

}
