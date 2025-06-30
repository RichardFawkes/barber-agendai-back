using MediatR;
using BarbeariaSaaS.Shared.DTOs.Request;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Features.Bookings.Commands;

public record UpdateBookingStatusCommand(
    Guid BookingId,
    UpdateBookingStatusDto UpdateDto
) : IRequest<BookingDto>; 