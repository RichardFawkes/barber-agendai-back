using MediatR;
using BarbeariaSaaS.Shared.DTOs.Request;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Features.Bookings.Commands;

public class CreateBookingCommand : IRequest<BookingDto>
{
    public Guid TenantId { get; set; }
    public Guid ServiceId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public TimeSpan BookingTime { get; set; }
    public string? Notes { get; set; }

    public CreateBookingCommand(CreateBookingDto dto)
    {
        TenantId = Guid.Parse(dto.TenantId);
        ServiceId = Guid.Parse(dto.ServiceId);
        CustomerName = dto.CustomerName;
        CustomerEmail = dto.CustomerEmail;
        CustomerPhone = dto.CustomerPhone;
        BookingDate = DateTime.Parse(dto.Date);
        BookingTime = TimeSpan.Parse(dto.Time);
        Notes = dto.Notes;
    }
} 