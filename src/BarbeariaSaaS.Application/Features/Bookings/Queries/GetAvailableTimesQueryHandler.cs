using MediatR;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Shared.DTOs.Response;
using BarbeariaSaaS.Domain.Entities;
using System.Globalization;

namespace BarbeariaSaaS.Application.Features.Bookings.Queries;

public class GetAvailableTimesQueryHandler : IRequestHandler<GetAvailableTimesQuery, AvailableTimesResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAvailableTimesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AvailableTimesResponseDto> Handle(GetAvailableTimesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validar e converter data
            if (!DateOnly.TryParse(request.Date, out var parsedDate))
            {
                return new AvailableTimesResponseDto
                {
                    Success = false,
                    Error = new ErrorDto
                    {
                        Code = "INVALID_DATE",
                        Message = "Data inválida. Use o formato YYYY-MM-DD"
                    }
                };
            }

            // Obter tenant pelo subdomain
            var tenants = await _unitOfWork.Tenants.FindAsync(t => t.Subdomain == request.Subdomain.ToLower());
            var tenant = tenants.FirstOrDefault();
            
            if (tenant == null)
            {
                return new AvailableTimesResponseDto
                {
                    Success = false,
                    Error = new ErrorDto
                    {
                        Code = "TENANT_NOT_FOUND",
                        Message = $"Tenant com subdomain '{request.Subdomain}' não encontrado"
                    }
                };
            }

            // Obter serviço
            var service = await _unitOfWork.Services.GetByIdAsync(request.ServiceId);
            if (service == null || !service.IsActive || service.TenantId != tenant.Id)
            {
                return new AvailableTimesResponseDto
                {
                    Success = false,
                    Error = new ErrorDto
                    {
                        Code = "SERVICE_NOT_FOUND",
                        Message = "Serviço não encontrado ou não está ativo"
                    }
                };
            }

            var dayOfWeek = (int)parsedDate.DayOfWeek;
            var dayName = parsedDate.DayOfWeek.ToString().ToLower();

            // Buscar informações do dia
            var businessHours = await _unitOfWork.BusinessHours.FindAsync(bh => bh.TenantId == tenant.Id);
            var businessHour = businessHours.FirstOrDefault(bh => bh.DayOfWeek == dayOfWeek);

            // Verificar se está aberto
            if (businessHour == null || !businessHour.IsOpen)
            {
                return new AvailableTimesResponseDto
                {
                    Success = true,
                    Data = new AvailableTimesDataDto
                    {
                        Date = request.Date,
                        DayOfWeek = dayName,
                        ServiceId = request.ServiceId.ToString(),
                        ServiceName = service.Name,
                        ServiceDuration = service.DurationMinutes,
                        AvailableTimes = new List<string>(),
                        BusinessHours = new BusinessHoursDto { IsOpen = false, DayOfWeek = dayOfWeek },
                        Summary = new AvailabilityDaySummaryDto()
                    }
                };
            }

            // Verificar dia especial
            var specialDays = await _unitOfWork.SpecialDays.FindAsync(sd => 
                sd.TenantId == tenant.Id && sd.Date == parsedDate);
            var specialDay = specialDays.FirstOrDefault();

            TimeSpan openTime, closeTime;
            if (specialDay != null)
            {
                if (!specialDay.IsOpen)
                {
                    return new AvailableTimesResponseDto
                    {
                        Success = true,
                        Data = new AvailableTimesDataDto
                        {
                            Date = request.Date,
                            DayOfWeek = dayName,
                            ServiceId = request.ServiceId.ToString(),
                            ServiceName = service.Name,
                            ServiceDuration = service.DurationMinutes,
                            AvailableTimes = new List<string>(),
                            BusinessHours = new BusinessHoursDto { IsOpen = false, DayOfWeek = dayOfWeek },
                            Summary = new AvailabilityDaySummaryDto()
                        }
                    };
                }

                openTime = specialDay.CustomStartTime ?? businessHour.OpenTime;
                closeTime = specialDay.CustomEndTime ?? businessHour.CloseTime;
            }
            else
            {
                openTime = businessHour.OpenTime;
                closeTime = businessHour.CloseTime;
            }

            // Verificar bloqueio manual do dia inteiro
            var manualBlocks = await _unitOfWork.ManualBlocks.FindAsync(mb => 
                mb.TenantId == tenant.Id && mb.Date == parsedDate);
            var fullDayBlock = manualBlocks.FirstOrDefault(mb => mb.Type == ManualBlockType.FullDayBlock);
            
            if (fullDayBlock != null)
            {
                return new AvailableTimesResponseDto
                {
                    Success = true,
                    Data = new AvailableTimesDataDto
                    {
                        Date = request.Date,
                        DayOfWeek = dayName,
                        ServiceId = request.ServiceId.ToString(),
                        ServiceName = service.Name,
                        ServiceDuration = service.DurationMinutes,
                        AvailableTimes = new List<string>(),
                        BusinessHours = new BusinessHoursDto 
                        { 
                            IsOpen = false, 
                            DayOfWeek = dayOfWeek,
                            Start = TimeOnly.FromTimeSpan(openTime).ToString("HH:mm"),
                            End = TimeOnly.FromTimeSpan(closeTime).ToString("HH:mm")
                        },
                        Summary = new AvailabilityDaySummaryDto()
                    }
                };
            }

            // Buscar pausas
            var businessBreaks = await _unitOfWork.BusinessBreaks.FindAsync(bb => 
                bb.TenantId == tenant.Id && bb.AppliesToAllDays);

            // Buscar agendamentos do dia
            var bookingDate = parsedDate.ToDateTime(TimeOnly.MinValue);
            var bookings = await _unitOfWork.Bookings.FindAsync(b => 
                b.TenantId == tenant.Id && 
                b.BookingDate.Date == bookingDate.Date && 
                b.Status != BookingStatus.Cancelled);

            // Obter configurações do tenant
            var tenantSettings = await _unitOfWork.TenantSettings.FindAsync(ts => ts.TenantId == tenant.Id);
            var settings = tenantSettings.FirstOrDefault();
            var slotDuration = settings?.SlotDurationMinutes ?? 30;

            // Gerar todos os slots possíveis
            var allSlots = GenerateTimeSlots(
                TimeOnly.FromTimeSpan(openTime), 
                TimeOnly.FromTimeSpan(closeTime), 
                slotDuration, 
                service.DurationMinutes);

            // Remover slots ocupados por pausas
            var breaks = businessBreaks.Select(bb => new BusinessBreakDto
            {
                Start = TimeOnly.FromTimeSpan(bb.StartTime).ToString("HH:mm"),
                End = TimeOnly.FromTimeSpan(bb.EndTime).ToString("HH:mm"),
                Name = bb.Name
            }).ToList();

            var availableSlots = allSlots.ToList();
            foreach (var breakTime in businessBreaks)
            {
                var breakStart = TimeOnly.FromTimeSpan(breakTime.StartTime);
                var breakEnd = TimeOnly.FromTimeSpan(breakTime.EndTime);
                availableSlots.RemoveAll(slot => slot >= breakStart && slot < breakEnd);
            }

            // Remover slots ocupados por bloqueios manuais
            var temporaryBlocks = manualBlocks.Where(mb => mb.Type == ManualBlockType.TemporaryBlock);
            foreach (var block in temporaryBlocks)
            {
                if (block.StartTime.HasValue && block.EndTime.HasValue)
                {
                    var blockStart = TimeOnly.FromTimeSpan(block.StartTime.Value);
                    var blockEnd = TimeOnly.FromTimeSpan(block.EndTime.Value);
                    availableSlots.RemoveAll(slot => slot >= blockStart && slot < blockEnd);
                }
            }

            // Processar agendamentos existentes
            var occupiedSlots = new List<OccupiedSlotDto>();
            foreach (var booking in bookings)
            {
                var bookingTime = TimeOnly.FromTimeSpan(booking.BookingTime);
                var bookingService = await _unitOfWork.Services.GetByIdAsync(booking.ServiceId);
                var endTime = bookingTime.Add(TimeSpan.FromMinutes(bookingService?.DurationMinutes ?? 30));
                
                availableSlots.Remove(bookingTime);
                
                occupiedSlots.Add(new OccupiedSlotDto
                {
                    Time = bookingTime.ToString("HH:mm"),
                    EndTime = endTime.ToString("HH:mm"),
                    BookingId = booking.Id.ToString(),
                    CustomerName = booking.CustomerName,
                    ServiceName = booking.Service?.Name ?? bookingService?.Name ?? "Serviço",
                    Status = booking.Status.ToString()
                });
            }

            // Identificar slots bloqueados
            var blockedSlots = new List<BlockedSlotDto>();
            foreach (var slot in allSlots)
            {
                if (!availableSlots.Contains(slot) && !occupiedSlots.Any(o => TimeOnly.Parse(o.Time) == slot))
                {
                    var reason = "BLOCKED";
                    var details = "Horário bloqueado";

                    // Verificar se é por pausa
                    foreach (var breakTime in businessBreaks)
                    {
                        var breakStart = TimeOnly.FromTimeSpan(breakTime.StartTime);
                        var breakEnd = TimeOnly.FromTimeSpan(breakTime.EndTime);
                        if (slot >= breakStart && slot < breakEnd)
                        {
                            reason = "BREAK";
                            details = breakTime.Name;
                            break;
                        }
                    }

                    // Verificar se é por bloqueio manual
                    foreach (var block in temporaryBlocks)
                    {
                        if (block.StartTime.HasValue && block.EndTime.HasValue)
                        {
                            var blockStart = TimeOnly.FromTimeSpan(block.StartTime.Value);
                            var blockEnd = TimeOnly.FromTimeSpan(block.EndTime.Value);
                            if (slot >= blockStart && slot < blockEnd)
                            {
                                reason = "MANUAL_BLOCK";
                                details = block.Reason;
                                break;
                            }
                        }
                    }

                    blockedSlots.Add(new BlockedSlotDto
                    {
                        Time = slot.ToString("HH:mm"),
                        Reason = reason,
                        Details = details
                    });
                }
            }

            return new AvailableTimesResponseDto
            {
                Success = true,
                Data = new AvailableTimesDataDto
                {
                    Date = request.Date,
                    DayOfWeek = dayName,
                    ServiceId = request.ServiceId.ToString(),
                    ServiceName = service.Name,
                    ServiceDuration = service.DurationMinutes,
                    AvailableTimes = availableSlots.Select(s => s.ToString("HH:mm")).ToList(),
                    BusinessHours = new BusinessHoursDto
                    {
                        IsOpen = true,
                        Start = TimeOnly.FromTimeSpan(openTime).ToString("HH:mm"),
                        End = TimeOnly.FromTimeSpan(closeTime).ToString("HH:mm"),
                        DayOfWeek = dayOfWeek
                    },
                    Breaks = breaks,
                    OccupiedSlots = occupiedSlots,
                    BlockedSlots = blockedSlots,
                    Summary = new AvailabilityDaySummaryDto
                    {
                        TotalSlots = allSlots.Count,
                        AvailableSlots = availableSlots.Count,
                        OccupiedSlots = occupiedSlots.Count,
                        BlockedSlots = blockedSlots.Count
                    }
                }
            };
        }
        catch (Exception ex)
        {
            return new AvailableTimesResponseDto
            {
                Success = false,
                Error = new ErrorDto
                {
                    Code = "INTERNAL_ERROR",
                    Message = "Erro interno do servidor",
                    Details = ex.Message
                }
            };
        }
    }

    private List<TimeOnly> GenerateTimeSlots(TimeOnly openTime, TimeOnly closeTime, int slotDuration, int serviceDuration)
    {
        var slots = new List<TimeOnly>();
        var current = openTime;
        var slotSpan = TimeSpan.FromMinutes(slotDuration);
        var serviceSpan = TimeSpan.FromMinutes(serviceDuration);

        while (current.Add(serviceSpan) <= closeTime)
        {
            slots.Add(current);
            current = current.Add(slotSpan);
        }

        return slots;
    }
} 