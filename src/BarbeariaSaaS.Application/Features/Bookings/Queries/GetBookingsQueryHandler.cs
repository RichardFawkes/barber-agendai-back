using MediatR;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Shared.DTOs.Response;
using BarbeariaSaaS.Domain.Entities;

namespace BarbeariaSaaS.Application.Features.Bookings.Queries;

public class GetBookingsQueryHandler : IRequestHandler<GetBookingsQuery, IEnumerable<BookingDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBookingsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<BookingDto>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        // Obter todos os agendamentos do tenant
        var allBookings = await _unitOfWork.Bookings.FindAsync(b => b.TenantId == request.TenantId);
        
        // Aplicar filtros
        var filteredBookings = allBookings.AsEnumerable();

        if (request.StartDate.HasValue)
        {
            var startDateTime = request.StartDate.Value.ToDateTime(TimeOnly.MinValue);
            filteredBookings = filteredBookings.Where(b => b.BookingDate.Date >= startDateTime.Date);
        }

        if (request.EndDate.HasValue)
        {
            var endDateTime = request.EndDate.Value.ToDateTime(TimeOnly.MinValue);
            filteredBookings = filteredBookings.Where(b => b.BookingDate.Date <= endDateTime.Date);
        }

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<BookingStatus>(request.Status, true, out var status))
        {
            filteredBookings = filteredBookings.Where(b => b.Status == status);
        }

        // Ordenar por data e hora
        var bookings = filteredBookings
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.BookingTime)
            .ToList();

        // Mapear para DTO
        var result = new List<BookingDto>();
        
        foreach (var booking in bookings)
        {
            // Obter serviço
            var service = await _unitOfWork.Services.GetByIdAsync(booking.ServiceId);
            
            // Obter customer
            Customer? customer = null;
            if (booking.CustomerId.HasValue)
            {
                customer = await _unitOfWork.Customers.GetByIdAsync(booking.CustomerId.Value);
            }

            result.Add(new BookingDto
            {
                Id = booking.Id.ToString(),
                Date = DateOnly.FromDateTime(booking.BookingDate).ToString("yyyy-MM-dd"),
                Time = TimeOnly.FromTimeSpan(booking.BookingTime).ToString("HH:mm"),
                Status = booking.Status.ToString().ToLower(),
                CustomerName = customer?.Name ?? booking.CustomerName,
                CustomerEmail = customer?.Email ?? booking.CustomerEmail,
                CustomerPhone = customer?.Phone ?? booking.CustomerPhone,
                ServiceName = service?.Name ?? "Serviço não informado",
                ServicePrice = service?.Price ?? booking.ServicePrice,
                ServiceDuration = service?.DurationMinutes ?? 0,
                Notes = booking.Notes,
                CreatedAt = booking.CreatedAt
            });
        }

        return result;
    }
} 