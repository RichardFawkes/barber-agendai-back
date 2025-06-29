using MediatR;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Domain.Entities;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Features.Dashboard.Queries;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.Today;

        // Get statistics in parallel for better performance
        var totalBookingsTask = _unitOfWork.Bookings.CountAsync(b => b.TenantId == request.TenantId);
        var todayRevenueTask = _unitOfWork.Bookings.GetTotalRevenueAsync(request.TenantId, today, today);
        var totalClientsTask = _unitOfWork.Customers.CountAsync(c => c.TenantId == request.TenantId);
        var pendingBookingsTask = _unitOfWork.Bookings.GetBookingCountByStatusAsync(request.TenantId, BookingStatus.Pending);
        var confirmedBookingsTask = _unitOfWork.Bookings.GetBookingCountByStatusAsync(request.TenantId, BookingStatus.Confirmed);

        await Task.WhenAll(totalBookingsTask, todayRevenueTask, totalClientsTask, pendingBookingsTask, confirmedBookingsTask);

        return new DashboardStatsDto
        {
            TotalBookings = await totalBookingsTask,
            TodayRevenue = await todayRevenueTask,
            TotalClients = await totalClientsTask,
            AverageRating = 4.5m, // TODO: Implement rating system
            PendingBookings = await pendingBookingsTask,
            ConfirmedBookings = await confirmedBookingsTask
        };
    }
} 