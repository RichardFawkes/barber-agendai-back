using MediatR;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Features.Tenants.Queries;

public class GetTenantBySubdomainQuery : IRequest<TenantDto?>
{
    public string Subdomain { get; set; } = string.Empty;

    public GetTenantBySubdomainQuery(string subdomain)
    {
        Subdomain = subdomain;
    }
} 