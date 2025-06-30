using MediatR;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Shared.DTOs.Response;
using BarbeariaSaaS.Domain.Entities;
using System.Globalization;

namespace BarbeariaSaaS.Application.Features.Bookings.Queries;

public class GetAvailableDatesQueryHandler : IRequestHandler<GetAvailableDatesQuery, AvailableDatesResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAvailableDatesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AvailableDatesResponseDto> Handle(GetAvailableDatesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Obter tenant pelo subdomain
            var tenants = await _unitOfWork.Tenants.FindAsync(t => t.Subdomain == request.Subdomain.ToLower());
            var tenant = tenants.FirstOrDefault();
            
            if (tenant == null)
            {
                return new AvailableDatesResponseDto
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
                return new AvailableDatesResponseDto
                {
                    Success = false,
                    Error = new ErrorDto
                    {
                        Code = "SERVICE_NOT_FOUND",
                        Message = "Serviço não encontrado ou não está ativo"
                    }
                };
            }

            // Validar mês e ano
            if (request.Month < 1 || request.Month > 12 || request.Year < DateTime.Now.Year)
            {
                return new AvailableDatesResponseDto
                {
                    Success = false,
                    Error = new ErrorDto
                    {
                        Code = "INVALID_DATE_RANGE",
                        Message = "Mês ou ano inválido"
                    }
                };
            }

            var startDate = new DateOnly(request.Year, request.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var totalDaysInMonth = DateTime.DaysInMonth(request.Year, request.Month);

            // Buscar todas as informações necessárias em consultas otimizadas
            var businessHours = await _unitOfWork.BusinessHours.FindAsync(bh => bh.TenantId == tenant.Id);
            var specialDays = await _unitOfWork.SpecialDays.FindAsync(sd => 
                sd.TenantId == tenant.Id && 
                sd.Date >= startDate && 
                sd.Date <= endDate);
            var manualBlocks = await _unitOfWork.ManualBlocks.FindAsync(mb => 
                mb.TenantId == tenant.Id && 
                mb.Date >= startDate && 
                mb.Date <= endDate);

            // Buscar agendamentos do mês em uma única consulta
            var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
            var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue);
            var bookings = await _unitOfWork.Bookings.FindAsync(b => 
                b.TenantId == tenant.Id &&
                b.BookingDate >= startDateTime &&
                b.BookingDate <= endDateTime &&
                b.Status != BookingStatus.Cancelled);

            var availableDates = new List<AvailableDateDto>();
            var totalAvailableSlots = 0;
            var totalBookedSlots = 0;
            var availableDaysCount = 0;

            // Processar cada dia do mês
            for (int day = 1; day <= totalDaysInMonth; day++)
            {
                var currentDate = new DateOnly(request.Year, request.Month, day);
                var dayOfWeek = (int)currentDate.DayOfWeek;
                var dayName = currentDate.DayOfWeek.ToString().ToLower();

                // Verificar dia especial
                var specialDay = specialDays.FirstOrDefault(sd => sd.Date == currentDate);
                if (specialDay != null && !specialDay.IsOpen)
                {
                    availableDates.Add(new AvailableDateDto
                    {
                        Date = currentDate.ToString("yyyy-MM-dd"),
                        DayOfWeek = dayName,
                        HasAvailability = false,
                        TotalSlots = 0,
                        AvailableSlots = 0,
                        BookedSlots = 0,
                        Reason = specialDay.Type.ToString().ToUpper(),
                        ReasonDetail = specialDay.Name
                    });
                    continue;
                }

                // Verificar bloqueio manual do dia inteiro
                var fullDayBlock = manualBlocks.FirstOrDefault(mb => 
                    mb.Date == currentDate && mb.Type == ManualBlockType.FullDayBlock);
                if (fullDayBlock != null)
                {
                    availableDates.Add(new AvailableDateDto
                    {
                        Date = currentDate.ToString("yyyy-MM-dd"),
                        DayOfWeek = dayName,
                        HasAvailability = false,
                        TotalSlots = 0,
                        AvailableSlots = 0,
                        BookedSlots = 0,
                        Reason = "BLOCKED",
                        ReasonDetail = fullDayBlock.Reason
                    });
                    continue;
                }

                // Verificar horário de funcionamento
                var businessHour = businessHours.FirstOrDefault(bh => bh.DayOfWeek == dayOfWeek);
                if (businessHour == null || !businessHour.IsOpen)
                {
                    availableDates.Add(new AvailableDateDto
                    {
                        Date = currentDate.ToString("yyyy-MM-dd"),
                        DayOfWeek = dayName,
                        HasAvailability = false,
                        TotalSlots = 0,
                        AvailableSlots = 0,
                        BookedSlots = 0,
                        Reason = "CLOSED",
                        ReasonDetail = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(currentDate.DayOfWeek)} - Fechado"
                    });
                    continue;
                }

                // Calcular disponibilidade básica para o dia
                var dayBookings = bookings.Where(b => DateOnly.FromDateTime(b.BookingDate) == currentDate).Count();
                var estimatedSlots = CalculateEstimatedSlots(businessHour, service.DurationMinutes);
                var availableSlots = Math.Max(0, estimatedSlots - dayBookings);

                totalAvailableSlots += availableSlots;
                totalBookedSlots += dayBookings;

                if (availableSlots > 0)
                    availableDaysCount++;

                availableDates.Add(new AvailableDateDto
                {
                    Date = currentDate.ToString("yyyy-MM-dd"),
                    DayOfWeek = dayName,
                    HasAvailability = availableSlots > 0,
                    TotalSlots = estimatedSlots,
                    AvailableSlots = availableSlots,
                    BookedSlots = dayBookings,
                    FirstAvailableTime = businessHour.OpenTime.ToString(@"hh\:mm"),
                    LastAvailableTime = businessHour.CloseTime.Add(TimeSpan.FromMinutes(-service.DurationMinutes)).ToString(@"hh\:mm")
                });
            }

            var occupationRate = totalAvailableSlots > 0 ? 
                Math.Round((decimal)totalBookedSlots / (totalAvailableSlots + totalBookedSlots) * 100, 1) : 0;

            return new AvailableDatesResponseDto
            {
                Success = true,
                Data = new AvailableDatesDataDto
                {
                    Year = request.Year,
                    Month = request.Month,
                    ServiceId = request.ServiceId.ToString(),
                    ServiceName = service.Name,
                    ServiceDuration = service.DurationMinutes,
                    AvailableDates = availableDates,
                    Summary = new AvailabilityMonthSummaryDto
                    {
                        TotalDaysInMonth = totalDaysInMonth,
                        AvailableDays = availableDaysCount,
                        ClosedDays = totalDaysInMonth - availableDaysCount,
                        TotalAvailableSlots = totalAvailableSlots,
                        TotalBookedSlots = totalBookedSlots,
                        OccupationRate = occupationRate
                    }
                }
            };
        }
        catch (Exception ex)
        {
            return new AvailableDatesResponseDto
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

    private int CalculateEstimatedSlots(BusinessHour businessHour, int serviceDurationMinutes)
    {
        var workingHours = businessHour.CloseTime - businessHour.OpenTime;
        var workingMinutes = (int)workingHours.TotalMinutes;
        return Math.Max(0, workingMinutes / 30); // Slots de 30 minutos
    }
} 