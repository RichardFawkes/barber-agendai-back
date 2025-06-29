using MediatR;
using BarbeariaSaaS.Shared.DTOs.Request;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Features.Services.Commands;

public record CreateServiceCommand(CreateServiceDto Request) : IRequest<ServiceDto>; 