using MediatR;
using BarbeariaSaaS.Shared.DTOs.Request;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace BarbeariaSaaS.Application.Features.Tenants.Commands;

public class ResponseDto
{
    public bool Success { get; set; }
    public object? Data { get; set; }
    public ErrorDto? Error { get; set; }
}

public class ErrorDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public record UpdateBusinessHoursCommand(string Subdomain, UpdateBusinessHoursDto UpdateDto) : IRequest<ResponseDto>;

public class UpdateBusinessHoursCommandHandler : IRequestHandler<UpdateBusinessHoursCommand, ResponseDto>
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

    public async Task<ResponseDto> Handle(UpdateBusinessHoursCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting business hours update for subdomain: {Subdomain}", request.Subdomain);

            // Find tenant by subdomain
            var tenant = await _tenantRepository.GetBySubdomainAsync(request.Subdomain.ToLower());
            if (tenant == null)
            {
                _logger.LogWarning("Tenant not found for subdomain: {Subdomain}", request.Subdomain);
                return new ResponseDto
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

            // Now let's try to save them
            try
            {
                // Simple approach: Load tenant with business hours explicitly
                var tenantForUpdate = await _tenantRepository.GetByIdAsync(tenant.Id);
                if (tenantForUpdate == null)
                {
                    throw new InvalidOperationException("Tenant not found for update");
                }

                // Clear existing business hours using a simple loop
                var existingHoursCount = tenantForUpdate.BusinessHours.Count;
                var existingHoursList = tenantForUpdate.BusinessHours.ToList();
                
                foreach (var existingHour in existingHoursList)
                {
                    tenantForUpdate.BusinessHours.Remove(existingHour);
                }
                
                _logger.LogInformation("Removed {Count} existing business hours", existingHoursCount);

                // Add new business hours
                foreach (var newHour in newBusinessHours)
                {
                    tenantForUpdate.BusinessHours.Add(newHour);
                }
                _logger.LogInformation("Added {Count} new business hours", newBusinessHours.Count);

                // Update and save
                tenantForUpdate.UpdatedAt = DateTime.UtcNow;
                _tenantRepository.Update(tenantForUpdate);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Business hours successfully saved for tenant {TenantId}", tenant.Id);

                return new ResponseDto
                {
                    Success = true,
                    Data = new { 
                        message = "Horários de funcionamento atualizados com sucesso",
                        tenantId = tenant.Id,
                        savedHours = newBusinessHours.Count,
                        removedHours = existingHoursCount
                    }
                };
            }
            catch (Exception saveEx)
            {
                _logger.LogError(saveEx, "Error saving business hours for tenant {TenantId}: {Message}", 
                    tenant.Id, saveEx.Message);
                
                return new ResponseDto
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
            
            return new ResponseDto
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