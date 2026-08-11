using FinderStore.Backend.Application.Common.Interfaces;
using FinderStore.Backend.Application.DTOs;
using FinderStore.Backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinderStore.Backend.Application.Features.Orders.Queries
{

    public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>
    {
    }

    public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
    {
        private readonly IApplicationDbContext _context;

        public GetDashboardStatsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var totalOrders = await _context.Orders.CountAsync(cancellationToken);
            var totalProducts = await _context.Products.CountAsync(cancellationToken);
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.Total, cancellationToken);
            var pendingOrders = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);

            var recentOrders = await _context.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new RecentOrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.User.FullName,
                    Total = o.Total,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync(cancellationToken);

            var revenueData = await _context.Orders
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
                TotalCustomers = await _context.Users.CountAsync(cancellationToken),
                TotalRevenue = totalRevenue,
                PendingOrders = pendingOrders,
                RecentOrders = recentOrders,
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
