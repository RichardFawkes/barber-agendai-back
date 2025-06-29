using MediatR;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Features.Services.Queries;

public class GetActiveServicesQuery : IRequest<IEnumerable<ServiceDto>>
{
    public Guid TenantId { get; set; }

    public GetActiveServicesQuery(Guid tenantId)
    {
        TenantId = tenantId;
    }
} 