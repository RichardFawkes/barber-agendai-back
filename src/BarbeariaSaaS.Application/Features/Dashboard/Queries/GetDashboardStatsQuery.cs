using MediatR;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Features.Dashboard.Queries;

public class GetDashboardStatsQuery : IRequest<DashboardStatsDto>
{
    public Guid TenantId { get; set; }

    public GetDashboardStatsQuery(Guid tenantId)
    {
        TenantId = tenantId;
    }
} 