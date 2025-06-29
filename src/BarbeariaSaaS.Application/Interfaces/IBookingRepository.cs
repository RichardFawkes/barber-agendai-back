using BarbeariaSaaS.Domain.Entities;

namespace BarbeariaSaaS.Application.Interfaces;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IEnumerable<Booking>> GetTodayBookingsAsync(Guid tenantId);
    Task<IEnumerable<Booking>> GetBookingsByDateRangeAsync(Guid tenantId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Booking>> GetBookingsByStatusAsync(Guid tenantId, BookingStatus status);
    Task<bool> IsTimeSlotAvailableAsync(Guid tenantId, DateTime date, TimeSpan time, int durationMinutes, Guid? excludeBookingId = null);
    Task<decimal> GetTotalRevenueAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null);
    Task<int> GetBookingCountByStatusAsync(Guid tenantId, BookingStatus status);
} 