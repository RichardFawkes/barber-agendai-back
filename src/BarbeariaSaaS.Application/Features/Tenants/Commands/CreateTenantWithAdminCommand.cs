using MediatR;
using BarbeariaSaaS.Shared.DTOs.Request;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Features.Tenants.Commands;

public record CreateTenantWithAdminCommand(CreateTenantWithAdminDto Request) : IRequest<CreateTenantResponseDto>; 