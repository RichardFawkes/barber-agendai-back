using MediatR;
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
        var businessHours = await _unitOfWork.BusinessHours.FindAsync(bh => bh.TenantId == request.TenantId);
        var businessHour = businessHours.FirstOrDefault(bh => bh.DayOfWeek == dayOfWeek);

        if (businessHour == null || !businessHour.IsOpen)
            return Enumerable.Empty<TimeOnly>();

        // Obter agendamentos existentes para o dia
        var bookingDate = request.Date.ToDateTime(TimeOnly.MinValue);
        var existingBookings = await _unitOfWork.Bookings.FindAsync(b => 
            b.TenantId == request.TenantId && 
            b.BookingDate.Date == bookingDate.Date && 
            b.Status != Domain.Entities.BookingStatus.Cancelled);

        // Gerar slots de tempo disponíveis
        var availableTimes = new List<TimeOnly>();
        var openTime = TimeOnly.FromTimeSpan(businessHour.OpenTime);
        var closeTime = TimeOnly.FromTimeSpan(businessHour.CloseTime);
        var serviceDuration = TimeSpan.FromMinutes(service.DurationMinutes);

        var currentTime = openTime;
        while (currentTime.Add(serviceDuration) <= closeTime)
        {
            // Verificar se não há conflito com agendamentos existentes
            bool hasConflict = false;
            
            foreach (var booking in existingBookings)
            {
                var bookingStart = TimeOnly.FromTimeSpan(booking.BookingTime);
                var bookingService = await _unitOfWork.Services.GetByIdAsync(booking.ServiceId);
                var bookingDuration = TimeSpan.FromMinutes(bookingService?.DurationMinutes ?? 30);
                var bookingEnd = bookingStart.Add(bookingDuration);
                var proposedEnd = currentTime.Add(serviceDuration);

                // Verificar sobreposição
                if ((currentTime >= bookingStart && currentTime < bookingEnd) ||
                    (proposedEnd > bookingStart && proposedEnd <= bookingEnd) ||
                    (currentTime <= bookingStart && proposedEnd >= bookingEnd))
                {
                    hasConflict = true;
                    break;
                }
            }

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