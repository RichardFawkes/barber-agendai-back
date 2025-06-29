using AutoMapper;
using MediatR;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Features.Tenants.Queries;

public class GetTenantBySubdomainQueryHandler : IRequestHandler<GetTenantBySubdomainQuery, TenantDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTenantBySubdomainQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TenantDto?> Handle(GetTenantBySubdomainQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _unitOfWork.Tenants.GetBySubdomainAsync(request.Subdomain);
        
        if (tenant == null)
        {
            return null;
        }

        return _mapper.Map<TenantDto>(tenant);
    }
} 