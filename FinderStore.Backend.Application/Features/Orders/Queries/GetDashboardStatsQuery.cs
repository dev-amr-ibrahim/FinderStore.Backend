using FinderStore.Backend.Application.DTOs;
using FinderStore.Backend.Domain.Common.Interfaces;
using FinderStore.Backend.Domain.Entities;
using FinderStore.Backend.Domain.Enums;
using FinderStore.Backend.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace FinderStore.Backend.Application.Features.Orders.Queries
{

    public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>
    {
    }

    public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetDashboardStatsQueryHandler(IOrderRepository orderRepository, IProductRepository productRepository, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var totalOrders = await _orderRepository.CountAsync(cancellationToken);
            var totalProducts = await _productRepository.CountAsync(cancellationToken);
            var totalRevenue = await _orderRepository.GetTotalRevenueAsync(cancellationToken);
            var pendingOrders = await _orderRepository.GetPendingOrdersCountAsync(cancellationToken);

            var recentOrders = await _orderRepository.GetRecentOrdersAsync(5, cancellationToken);

            var revenueData = await _orderRepository.Query()
                .Where(o => o.CreatedAt >= thirtyDaysAgo && o.Status == OrderStatus.Delivered)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.Total)
                })
                .OrderBy(x => x.Date)
                .ToListAsync(cancellationToken);

            var stats = new DashboardStatsDto
            {
                TotalOrders = totalOrders,
                TotalProducts = totalProducts,
                TotalCustomers = await _userManager.Users.CountAsync(cancellationToken),
                TotalRevenue = totalRevenue,
                PendingOrders = pendingOrders,
                RecentOrders = recentOrders.Select(o => new RecentOrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.User.FullName,
                    Total = o.Total,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt
                }).ToList(),
                RevenueChart = new RevenueChartDto
                {
                    Labels = revenueData.Select(x => x.Date.ToString("MMM dd")).ToList(),
                    Values = revenueData.Select(x => x.Revenue).ToList()
                }
            };

            return stats;
        }
    }
}
