using MediatR;
using Microsoft.Extensions.Logging;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Domain.Entities;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Features.Bookings.Commands;

public class UpdateBookingStatusCommandHandler : IRequestHandler<UpdateBookingStatusCommand, BookingDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateBookingStatusCommandHandler> _logger;

    public UpdateBookingStatusCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateBookingStatusCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BookingDto> Handle(UpdateBookingStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating booking {BookingId} status to {Status}", 
            request.BookingId, request.UpdateDto.Status);

        // Get the booking
        var booking = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId);
        if (booking == null)
        {
            _logger.LogWarning("Booking {BookingId} not found", request.BookingId);
            throw new ArgumentException($"Agendamento {request.BookingId} não encontrado");
        }

        // Parse and validate status
        if (!Enum.TryParse<BookingStatus>(request.UpdateDto.Status, true, out var newStatus))
        {
            _logger.LogWarning("Invalid status {Status} for booking {BookingId}", 
                request.UpdateDto.Status, request.BookingId);
            throw new ArgumentException($"Status '{request.UpdateDto.Status}' é inválido");
        }

        // Validate status transition
        if (!IsValidStatusTransition(booking.Status, newStatus))
        {
            _logger.LogWarning("Invalid status transition from {OldStatus} to {NewStatus} for booking {BookingId}", 
                booking.Status, newStatus, request.BookingId);
            throw new InvalidOperationException($"Não é possível alterar status de {booking.Status} para {newStatus}");
        }

        // Update booking
        var oldStatus = booking.Status;
        booking.Status = newStatus;
        booking.UpdatedAt = DateTime.UtcNow;

        // Add notes if provided
        if (!string.IsNullOrWhiteSpace(request.UpdateDto.Notes))
        {
            var existingNotes = booking.Notes ?? string.Empty;
            var timestamp = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm");
            var statusChange = $"[{timestamp}] Status alterado de {oldStatus} para {newStatus}";
            
            if (!string.IsNullOrWhiteSpace(request.UpdateDto.Reason))
            {
                statusChange += $" - Motivo: {request.UpdateDto.Reason}";
            }
            
            statusChange += $" - {request.UpdateDto.Notes}";
            
            booking.Notes = string.IsNullOrWhiteSpace(existingNotes) 
                ? statusChange 
                : $"{existingNotes}\n{statusChange}";
        }

        // Save changes
        _unitOfWork.Bookings.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Booking {BookingId} status updated from {OldStatus} to {NewStatus}", 
            request.BookingId, oldStatus, newStatus);

        // Return updated booking DTO
        return new BookingDto
        {
            Id = booking.Id.ToString(),
            TenantId = booking.TenantId.ToString(),
            ServiceId = booking.ServiceId.ToString(),
            CustomerName = booking.CustomerName,
            CustomerEmail = booking.CustomerEmail,
            CustomerPhone = booking.CustomerPhone,
            Date = booking.BookingDate.ToString("yyyy-MM-dd"),
            Time = booking.BookingTime.ToString(@"hh\:mm"),
            Status = booking.Status.ToString().ToLower(),
            Notes = booking.Notes,
            ServiceName = booking.Service?.Name ?? "",
            ServicePrice = booking.ServicePrice,
            ServiceDuration = booking.Service?.DurationMinutes ?? 0,
            CreatedAt = booking.CreatedAt,
            UpdatedAt = booking.UpdatedAt
        };
    }

    private static bool IsValidStatusTransition(BookingStatus currentStatus, BookingStatus newStatus)
    {
        // Allow same status (no change)
        if (currentStatus == newStatus)
            return true;

        return currentStatus switch
        {
            BookingStatus.Pending => newStatus is BookingStatus.Confirmed or BookingStatus.Cancelled,
            BookingStatus.Confirmed => newStatus is BookingStatus.InProgress or BookingStatus.Cancelled or BookingStatus.NoShow,
            BookingStatus.InProgress => newStatus is BookingStatus.Completed or BookingStatus.Cancelled,
            BookingStatus.Completed => false, // Completed bookings cannot be changed
            BookingStatus.Cancelled => false, // Cancelled bookings cannot be changed
            BookingStatus.NoShow => false,    // NoShow bookings cannot be changed
            _ => false
        };
    }
} 