using MediatR;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Features.Bookings.Queries;

public record GetAvailableTimesQuery(
    string Subdomain,
    Guid ServiceId,
    string Date
) : IRequest<AvailableTimesResponseDto>; 