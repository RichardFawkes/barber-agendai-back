using MediatR;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Features.Bookings.Queries;

public record GetAvailableDatesQuery(
    string Subdomain,
    Guid ServiceId,
    int Year,
    int Month
) : IRequest<AvailableDatesResponseDto>; 