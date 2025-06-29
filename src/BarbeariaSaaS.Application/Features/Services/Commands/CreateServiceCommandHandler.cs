using MediatR;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Domain.Entities;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Features.Services.Commands;

public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, ServiceDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceDto> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = new Service
        {
            Id = Guid.NewGuid(),
            TenantId = request.Request.TenantId,
            Name = request.Request.Name,
            Description = request.Request.Description,
            Price = request.Request.Price,
            DurationMinutes = request.Request.DurationMinutes,
            Color = request.Request.Color ?? "#3B82F6",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Services.AddAsync(service);
        await _unitOfWork.SaveChangesAsync();

        return new ServiceDto
        {
            Id = service.Id.ToString(),
            Name = service.Name,
            Description = service.Description,
            Price = service.Price,
            DurationMinutes = service.DurationMinutes,
            Color = service.Color,
            IsActive = service.IsActive
        };
    }
} 