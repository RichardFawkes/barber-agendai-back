using AutoMapper;
using MediatR;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Features.Services.Queries;

public class GetActiveServicesQueryHandler : IRequestHandler<GetActiveServicesQuery, IEnumerable<ServiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetActiveServicesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ServiceDto>> Handle(GetActiveServicesQuery request, CancellationToken cancellationToken)
    {
        var services = await _unitOfWork.Services.GetActiveServicesByTenantAsync(request.TenantId);
        return _mapper.Map<IEnumerable<ServiceDto>>(services);
    }
} 