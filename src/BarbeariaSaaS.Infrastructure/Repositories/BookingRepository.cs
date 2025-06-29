using Microsoft.EntityFrameworkCore;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Domain.Entities;
using BarbeariaSaaS.Infrastructure.Data;

namespace BarbeariaSaaS.Infrastructure.Repositories;

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Booking>> GetTodayBookingsAsync(Guid tenantId)
    {
        var today = DateTime.Today;
        return await _dbSet
            .Include(b => b.Service)
            .Include(b => b.Customer)
            .Where(b => b.TenantId == tenantId && b.BookingDate == today)
            .OrderBy(b => b.BookingTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsByDateRangeAsync(Guid tenantId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(b => b.Service)
            .Include(b => b.Customer)
            .Where(b => b.TenantId == tenantId && 
                       b.BookingDate >= startDate && 
                       b.BookingDate <= endDate)
            .OrderBy(b => b.BookingDate)
            .ThenBy(b => b.BookingTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsByStatusAsync(Guid tenantId, BookingStatus status)
    {
        return await _dbSet
            .Include(b => b.Service)
            .Include(b => b.Customer)
            .Where(b => b.TenantId == tenantId && b.Status == status)
            .OrderBy(b => b.BookingDate)
            .ThenBy(b => b.BookingTime)
            .ToListAsync();
    }

    public async Task<bool> IsTimeSlotAvailableAsync(Guid tenantId, DateTime date, TimeSpan time, int durationMinutes, Guid? excludeBookingId = null)
    {
        var endTime = time.Add(TimeSpan.FromMinutes(durationMinutes));
        
        var query = _dbSet.Where(b => b.TenantId == tenantId && 
                                     b.BookingDate == date &&
                                     b.Status != BookingStatus.Cancelled &&
                                     b.Status != BookingStatus.NoShow);
        
        if (excludeBookingId.HasValue)
        {
            query = query.Where(b => b.Id != excludeBookingId.Value);
        }

        var conflictingBookings = await query
            .Include(b => b.Service)
            .ToListAsync();

        foreach (var booking in conflictingBookings)
        {
            var bookingEndTime = booking.BookingTime.Add(TimeSpan.FromMinutes(booking.Service.DurationMinutes));
            
            // Check for time overlap
            if ((time < bookingEndTime) && (endTime > booking.BookingTime))
            {
                return false;
            }
        }

        return true;
    }

    public async Task<decimal> GetTotalRevenueAsync(Guid tenantId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _dbSet.Where(b => b.TenantId == tenantId && 
                                     b.Status == BookingStatus.Completed);

        if (startDate.HasValue)
        {
            query = query.Where(b => b.BookingDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(b => b.BookingDate <= endDate.Value);
        }

        return await query.SumAsync(b => b.ServicePrice);
    }

    public async Task<int> GetBookingCountByStatusAsync(Guid tenantId, BookingStatus status)
    {
        return await _dbSet
            .CountAsync(b => b.TenantId == tenantId && b.Status == status);
    }
} 