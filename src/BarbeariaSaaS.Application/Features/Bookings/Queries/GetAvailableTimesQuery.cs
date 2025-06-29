using MediatR;

namespace BarbeariaSaaS.Application.Features.Bookings.Queries;

public record GetAvailableTimesQuery(
    Guid TenantId,
    Guid ServiceId,
    DateOnly Date
) : IRequest<IEnumerable<TimeOnly>>; 