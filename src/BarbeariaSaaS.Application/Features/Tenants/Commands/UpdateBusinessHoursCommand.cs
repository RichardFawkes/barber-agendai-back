using MediatR;
using BarbeariaSaaS.Shared.DTOs.Request;
using BarbeariaSaaS.Shared.DTOs.Response;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace BarbeariaSaaS.Application.Features.Tenants.Commands;

public record UpdateBusinessHoursCommand(string Subdomain, UpdateBusinessHoursDto UpdateDto) : IRequest<BusinessHoursResponseDto>;

public class UpdateBusinessHoursCommandHandler : IRequestHandler<UpdateBusinessHoursCommand, BusinessHoursResponseDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateBusinessHoursCommandHandler> _logger;

    public UpdateBusinessHoursCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateBusinessHoursCommandHandler> logger)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BusinessHoursResponseDto> Handle(UpdateBusinessHoursCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting business hours update for subdomain: {Subdomain}", request.Subdomain);

            // Find tenant by subdomain
            var tenant = await _tenantRepository.GetBySubdomainAsync(request.Subdomain.ToLower());
            if (tenant == null)
            {
                _logger.LogWarning("Tenant not found for subdomain: {Subdomain}", request.Subdomain);
                return new BusinessHoursResponseDto
                {
                    Success = false,
                    Error = new ErrorDto
                    {
                        Code = "TENANT_NOT_FOUND",
                        Message = "Tenant não encontrado"
                    }
                };
            }

            _logger.LogInformation("Tenant found: {TenantId}, existing business hours count: {Count}", 
                tenant.Id, tenant.BusinessHours?.Count ?? 0);

            // Instead of clearing, let's just create new business hours for now
            var newBusinessHours = new List<BusinessHour>();

            // Add new business hours
            foreach (var scheduleItem in request.UpdateDto.Schedule ?? new List<BusinessHourConfigDto>())
            {
                _logger.LogInformation("Processing schedule item: Day {DayOfWeek}, IsOpen: {IsOpen}, StartTime: {StartTime}, EndTime: {EndTime}",
                    scheduleItem.DayOfWeek, scheduleItem.IsOpen, scheduleItem.StartTime, scheduleItem.EndTime);

                if (scheduleItem.IsOpen && !string.IsNullOrEmpty(scheduleItem.StartTime) && !string.IsNullOrEmpty(scheduleItem.EndTime))
                {
                    try
                    {
                        var newBusinessHour = new BusinessHour
                        {
                            Id = Guid.NewGuid(),
                            TenantId = tenant.Id,
                            DayOfWeek = scheduleItem.DayOfWeek,
                            IsOpen = scheduleItem.IsOpen,
                            OpenTime = TimeSpan.Parse(scheduleItem.StartTime),
                            CloseTime = TimeSpan.Parse(scheduleItem.EndTime)
                        };

                        newBusinessHours.Add(newBusinessHour);
                        _logger.LogInformation("Created business hour for day {DayOfWeek}", scheduleItem.DayOfWeek);
                    }
                    catch (Exception parseEx)
                    {
                        _logger.LogError(parseEx, "Error parsing times for day {DayOfWeek}: {StartTime} - {EndTime}", 
                            scheduleItem.DayOfWeek, scheduleItem.StartTime, scheduleItem.EndTime);
                        throw;
                    }
                }
            }

            _logger.LogInformation("Created {Count} new business hours", newBusinessHours.Count);

            // Use the specialized method to update business hours
            try
            {
                await _tenantRepository.UpdateBusinessHoursAsync(tenant.Id, newBusinessHours);

                _logger.LogInformation("Business hours successfully updated for tenant {TenantId}", tenant.Id);

                return new BusinessHoursResponseDto
                {
                    Success = true,
                    Data = new BusinessHoursDataDto
                    { 
                        Message = "Horários de funcionamento atualizados com sucesso",
                        TenantId = tenant.Id.ToString(),
                        SavedHours = newBusinessHours.Count
                    }
                };
            }
            catch (Exception saveEx)
            {
                _logger.LogError(saveEx, "Error saving business hours for tenant {TenantId}: {Message}", 
                    tenant.Id, saveEx.Message);
                
                return new BusinessHoursResponseDto
                {
                    Success = false,
                    Error = new ErrorDto
                    {
                        Code = "SAVE_ERROR",
                        Message = $"Erro ao salvar: {saveEx.Message}"
                    }
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in business hours update for subdomain {Subdomain}: {Message}", 
                request.Subdomain, ex.Message);
            
            return new BusinessHoursResponseDto
            {
                Success = false,
                Error = new ErrorDto
                {
                    Code = "INTERNAL_ERROR",
                    Message = $"Erro interno: {ex.Message}"
                }
            };
        }
    }
} 