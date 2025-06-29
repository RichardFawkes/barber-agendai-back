using MediatR;
using Microsoft.EntityFrameworkCore;
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
        var query = _unitOfWork.Context.Bookings
            .Include(b => b.Service)
            .Include(b => b.Customer)
            .Where(b => b.TenantId == request.TenantId);

        // Aplicar filtros
        if (request.StartDate.HasValue)
        {
            query = query.Where(b => b.Date >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(b => b.Date <= request.EndDate.Value);
        }

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<BookingStatus>(request.Status, true, out var status))
        {
            query = query.Where(b => b.Status == status);
        }

        var bookings = await query
            .OrderByDescending(b => b.Date)
            .ThenByDescending(b => b.Time)
            .ToListAsync(cancellationToken);

        return bookings.Select(b => new BookingDto
        {
            Id = b.Id.ToString(),
            Date = b.Date.ToString("yyyy-MM-dd"),
            Time = TimeOnly.FromTimeSpan(b.Time).ToString("HH:mm"),
            Status = b.Status.ToString().ToLower(),
            CustomerName = b.Customer?.Name ?? "Cliente não informado",
            CustomerEmail = b.Customer?.Email ?? "",
            CustomerPhone = b.Customer?.Phone ?? "",
            ServiceName = b.Service?.Name ?? "Serviço não informado",
            ServicePrice = b.Service?.Price ?? 0,
            ServiceDuration = b.Service?.DurationMinutes ?? 0,
            Notes = b.Notes,
            CreatedAt = b.CreatedAt
        });
    }
} 