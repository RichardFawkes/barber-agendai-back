using AutoMapper;
using MediatR;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Domain.Entities;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Features.Bookings.Commands;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateBookingCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BookingDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate service exists and belongs to tenant
        var service = await _unitOfWork.Services.GetByIdAsync(request.ServiceId);
        if (service == null || service.TenantId != request.TenantId)
        {
            throw new ArgumentException("Service not found or doesn't belong to the specified tenant");
        }

        // 2. Check if time slot is available
        var isAvailable = await _unitOfWork.Bookings.IsTimeSlotAvailableAsync(
            request.TenantId, 
            request.BookingDate, 
            request.BookingTime, 
            service.DurationMinutes);

        if (!isAvailable)
        {
            throw new InvalidOperationException("The selected time slot is not available");
        }

        // 3. Find or create customer
        var customer = await _unitOfWork.Customers.GetByEmailAsync(request.TenantId, request.CustomerEmail);
        if (customer == null)
        {
            customer = new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                Name = request.CustomerName,
                Email = request.CustomerEmail,
                Phone = request.CustomerPhone,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Customers.AddAsync(customer);
        }

        // 4. Create booking
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            ServiceId = request.ServiceId,
            CustomerId = customer.Id,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            BookingDate = request.BookingDate,
            BookingTime = request.BookingTime,
            ServicePrice = service.Price,
            Notes = request.Notes,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Bookings.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        // 5. Return mapped DTO
        var bookingWithService = await _unitOfWork.Bookings.GetByIdAsync(booking.Id);
        return _mapper.Map<BookingDto>(bookingWithService);
    }
} 