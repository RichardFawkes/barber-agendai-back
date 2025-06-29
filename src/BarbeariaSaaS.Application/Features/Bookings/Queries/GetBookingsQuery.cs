using MediatR;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Features.Bookings.Queries;

public record GetBookingsQuery(
    Guid TenantId,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    string? Status = null
) : IRequest<IEnumerable<BookingDto>>; 