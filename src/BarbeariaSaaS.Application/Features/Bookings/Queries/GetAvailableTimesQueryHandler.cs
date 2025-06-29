using MediatR;
using Microsoft.EntityFrameworkCore;
using BarbeariaSaaS.Application.Interfaces;

namespace BarbeariaSaaS.Application.Features.Bookings.Queries;

public class GetAvailableTimesQueryHandler : IRequestHandler<GetAvailableTimesQuery, IEnumerable<TimeOnly>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAvailableTimesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<TimeOnly>> Handle(GetAvailableTimesQuery request, CancellationToken cancellationToken)
    {
        // Obter o serviço para saber a duração
        var service = await _unitOfWork.Services.GetByIdAsync(request.ServiceId);
        if (service == null || !service.IsActive)
            return Enumerable.Empty<TimeOnly>();

        // Obter horário de funcionamento para o dia da semana
        var dayOfWeek = (int)request.Date.DayOfWeek;
        var businessHour = await _unitOfWork.Context.BusinessHours
            .FirstOrDefaultAsync(bh => bh.TenantId == request.TenantId && bh.DayOfWeek == dayOfWeek, cancellationToken);

        if (businessHour == null || !businessHour.IsOpen)
            return Enumerable.Empty<TimeOnly>();

        // Obter agendamentos existentes para o dia
        var existingBookings = await _unitOfWork.Context.Bookings
            .Where(b => b.TenantId == request.TenantId && 
                       b.Date == request.Date && 
                       b.Status != Domain.Entities.BookingStatus.Cancelled)
            .Select(b => new { b.Time, b.Service.DurationMinutes })
            .ToListAsync(cancellationToken);

        // Gerar slots de tempo disponíveis
        var availableTimes = new List<TimeOnly>();
        var openTime = TimeOnly.FromTimeSpan(businessHour.OpenTime);
        var closeTime = TimeOnly.FromTimeSpan(businessHour.CloseTime);
        var serviceDuration = TimeSpan.FromMinutes(service.DurationMinutes);

        var currentTime = openTime;
        while (currentTime.Add(serviceDuration) <= closeTime)
        {
            var currentTimeSpan = currentTime.ToTimeSpan();
            
            // Verificar se não há conflito com agendamentos existentes
            bool hasConflict = existingBookings.Any(booking =>
            {
                var bookingStart = TimeOnly.FromTimeSpan(booking.Time);
                var bookingEnd = bookingStart.Add(TimeSpan.FromMinutes(booking.DurationMinutes));
                var proposedEnd = currentTime.Add(serviceDuration);

                // Verificar sobreposição
                return (currentTime >= bookingStart && currentTime < bookingEnd) ||
                       (proposedEnd > bookingStart && proposedEnd <= bookingEnd) ||
                       (currentTime <= bookingStart && proposedEnd >= bookingEnd);
            });

            if (!hasConflict)
            {
                availableTimes.Add(currentTime);
            }

            // Incrementar em intervalos de 30 minutos
            currentTime = currentTime.Add(TimeSpan.FromMinutes(30));
        }

        return availableTimes;
    }
} 