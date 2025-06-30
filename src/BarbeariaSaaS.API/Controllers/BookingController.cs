using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BarbeariaSaaS.Application.Features.Bookings.Commands;
using BarbeariaSaaS.Application.Features.Bookings.Queries;
using BarbeariaSaaS.Shared.DTOs.Request;
using BarbeariaSaaS.Shared.DTOs.Response;
using BarbeariaSaaS.Infrastructure.Data;
using BarbeariaSaaS.API.Scripts;

namespace BarbeariaSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BookingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BookingController> _logger;
    private readonly ApplicationDbContext _context;

    public BookingController(IMediator mediator, ILogger<BookingController> logger, ApplicationDbContext context)
    {
        _mediator = mediator;
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// TEMPORARY: Seed test data
    /// </summary>
    [HttpPost("seed-test-data")]
    public async Task<ActionResult> SeedTestDataEndpoint()
    {
        try
        {
            await Scripts.SeedTestData.SeedAsync(_context);
            return Ok(new { message = "Dados de teste criados com sucesso!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Erro ao criar dados de teste: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get available dates for a month
    /// </summary>
    /// <param name="subdomain">Tenant subdomain</param>
    /// <param name="serviceId">Service ID</param>
    /// <param name="year">Year</param>
    /// <param name="month">Month (1-12)</param>
    /// <returns>Available dates with availability information</returns>
    [HttpGet("available-dates/{subdomain}")]
    [ProducesResponseType(typeof(AvailableDatesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvailableDatesResponseDto>> GetAvailableDates(
        string subdomain,
        [FromQuery] string serviceId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(subdomain))
            {
                return BadRequest(new { message = "Subdomain é obrigatório" });
            }

            if (!Guid.TryParse(serviceId, out var parsedServiceId))
            {
                return BadRequest(new { message = "Service ID deve ser um GUID válido" });
            }

            if (month < 1 || month > 12)
            {
                return BadRequest(new { message = "Mês deve estar entre 1 e 12" });
            }

            if (year < DateTime.Now.Year)
            {
                return BadRequest(new { message = "Ano não pode ser anterior ao ano atual" });
            }

            var query = new GetAvailableDatesQuery(subdomain, parsedServiceId, year, month);
            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                if (result.Error?.Code == "TENANT_NOT_FOUND")
                    return NotFound(result);
                return BadRequest(result);
            }

            _logger.LogInformation("Retrieved available dates for subdomain {Subdomain}, service {ServiceId}, {Year}-{Month}",
                subdomain, serviceId, year, month);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available dates for subdomain {Subdomain}", subdomain);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Get available time slots for booking
    /// </summary>
    /// <param name="subdomain">Tenant subdomain</param>
    /// <param name="serviceId">Service ID</param>
    /// <param name="date">Date in YYYY-MM-DD format</param>
    /// <returns>Available time slots with detailed information</returns>
    [HttpGet("available-times/{subdomain}")]
    [ProducesResponseType(typeof(AvailableTimesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvailableTimesResponseDto>> GetAvailableTimes(
        string subdomain, 
        [FromQuery] string serviceId, 
        [FromQuery] string date)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(subdomain))
            {
                return BadRequest(new { message = "Subdomain é obrigatório" });
            }

            if (!Guid.TryParse(serviceId, out var parsedServiceId))
            {
                return BadRequest(new { message = "Service ID deve ser um GUID válido" });
            }

            if (string.IsNullOrWhiteSpace(date))
            {
                return BadRequest(new { message = "Data é obrigatória no formato YYYY-MM-DD" });
            }

            var query = new GetAvailableTimesQuery(subdomain, parsedServiceId, date);
            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                if (result.Error?.Code == "TENANT_NOT_FOUND")
                    return NotFound(result);
                return BadRequest(result);
            }

            _logger.LogInformation("Retrieved available times for subdomain {Subdomain}, service {ServiceId} on {Date}", 
                subdomain, serviceId, date);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available times for subdomain {Subdomain} on {Date}", subdomain, date);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Create a new booking
    /// </summary>
    /// <param name="createBookingDto">Booking details</param>
    /// <returns>Created booking information</returns>
    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> CreateBooking([FromBody] CreateBookingDto createBookingDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new CreateBookingCommand(createBookingDto);
            var result = await _mediator.Send(command);

            _logger.LogInformation("Booking created successfully for customer {CustomerEmail} on {Date} at {Time}", 
                createBookingDto.CustomerEmail, createBookingDto.Date, createBookingDto.Time);

            return CreatedAtAction(nameof(CreateBooking), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid booking data: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Booking conflict: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for customer {CustomerEmail}", createBookingDto.CustomerEmail);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a booking for public users (from booking page)
    /// </summary>
    /// <param name="subdomain">Tenant subdomain</param>
    /// <param name="createBookingDto">Booking details</param>
    /// <returns>Created booking information</returns>
    [HttpPost("public/{subdomain}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingDto>> CreatePublicBooking(string subdomain, [FromBody] CreateBookingDto createBookingDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(subdomain))
            {
                return BadRequest(new { message = "Subdomain é obrigatório" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get tenant by subdomain first
            var tenantQuery = new BarbeariaSaaS.Application.Features.Tenants.Queries.GetTenantBySubdomainQuery(subdomain.ToLower());
            var tenant = await _mediator.Send(tenantQuery);

            if (tenant == null)
            {
                return NotFound(new { message = "Tenant não encontrado" });
            }

            // Override tenant ID from subdomain
            createBookingDto.TenantId = tenant.Id;

            var command = new CreateBookingCommand(createBookingDto);
            var result = await _mediator.Send(command);

            _logger.LogInformation("Public booking created for subdomain {Subdomain}, customer {CustomerEmail} on {Date} at {Time}", 
                subdomain, createBookingDto.CustomerEmail, createBookingDto.Date, createBookingDto.Time);

            return CreatedAtAction(nameof(CreatePublicBooking), new { subdomain, id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid booking data for subdomain {Subdomain}: {Message}", subdomain, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Booking conflict for subdomain {Subdomain}: {Message}", subdomain, ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating public booking for subdomain {Subdomain}", subdomain);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Health check for booking service
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult Health()
    {
        return Ok(new
        {
            service = "Booking",
            status = "Healthy",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get bookings for dashboard (compatibility endpoint)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="startDate">Start date filter (YYYY-MM-DD)</param>
    /// <param name="endDate">End date filter (YYYY-MM-DD)</param>
    /// <param name="status">Status filter (confirmed, cancelled, completed)</param>
    /// <returns>List of bookings for dashboard</returns>
    [HttpGet("dashboard/{tenantId}")]
    // [Authorize] // Commented out temporarily for testing
    [ProducesResponseType(typeof(IEnumerable<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetDashboardBookings(
        string tenantId,
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null,
        [FromQuery] string? status = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tenantId) || !Guid.TryParse(tenantId, out var parsedTenantId))
            {
                return BadRequest(new { message = "Valid tenant ID is required" });
            }

            DateOnly? parsedStartDate = null;
            DateOnly? parsedEndDate = null;

            if (!string.IsNullOrEmpty(startDate) && !DateOnly.TryParse(startDate, out var tempStartDate))
            {
                return BadRequest(new { message = "Invalid start date format (use YYYY-MM-DD)" });
            }
            else if (!string.IsNullOrEmpty(startDate))
            {
                parsedStartDate = DateOnly.Parse(startDate);
            }

            if (!string.IsNullOrEmpty(endDate) && !DateOnly.TryParse(endDate, out var tempEndDate))
            {
                return BadRequest(new { message = "Invalid end date format (use YYYY-MM-DD)" });
            }
            else if (!string.IsNullOrEmpty(endDate))
            {
                parsedEndDate = DateOnly.Parse(endDate);
            }

            var query = new BarbeariaSaaS.Application.Features.Bookings.Queries.GetBookingsQuery(
                parsedTenantId, parsedStartDate, parsedEndDate, status);
            
            var result = await _mediator.Send(query);

            _logger.LogInformation("Retrieved {Count} dashboard bookings for tenant {TenantId}", result.Count(), tenantId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard bookings for tenant: {TenantId}", tenantId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update booking status (confirm, cancel, complete, etc.)
    /// </summary>
    /// <param name="bookingId">Booking ID</param>
    /// <param name="updateDto">Status update details</param>
    /// <returns>Updated booking information</returns>
    [HttpPut("{bookingId}/status")]
    // [Authorize] // Commented out temporarily for testing
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BookingDto>> UpdateBookingStatus(string bookingId, [FromBody] UpdateBookingStatusDto updateDto)
    {
        try
        {
            if (!Guid.TryParse(bookingId, out var parsedBookingId))
            {
                return BadRequest(new { message = "Booking ID deve ser um GUID válido" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new UpdateBookingStatusCommand(parsedBookingId, updateDto);
            var result = await _mediator.Send(command);

            _logger.LogInformation("Booking {BookingId} status updated to {Status}", bookingId, updateDto.Status);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid data for booking status update {BookingId}: {Message}", bookingId, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid operation for booking status update {BookingId}: {Message}", bookingId, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking status for {BookingId}", bookingId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Confirm a booking (shortcut endpoint)
    /// </summary>
    /// <param name="bookingId">Booking ID</param>
    /// <returns>Updated booking information</returns>
    [HttpPost("{bookingId}/confirm")]
    // [Authorize] // Commented out temporarily for testing
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDto>> ConfirmBooking(string bookingId)
    {
        try
        {
            if (!Guid.TryParse(bookingId, out var parsedBookingId))
            {
                return BadRequest(new { message = "Booking ID deve ser um GUID válido" });
            }

            var updateDto = new UpdateBookingStatusDto
            {
                Status = "confirmed",
                Notes = "Agendamento confirmado automaticamente",
                Reason = "Confirmação do sistema"
            };

            var command = new UpdateBookingStatusCommand(parsedBookingId, updateDto);
            var result = await _mediator.Send(command);

            _logger.LogInformation("Booking {BookingId} confirmed", bookingId);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Error confirming booking {BookingId}: {Message}", bookingId, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid operation confirming booking {BookingId}: {Message}", bookingId, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming booking {BookingId}", bookingId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Cancel a booking (shortcut endpoint)
    /// </summary>
    /// <param name="bookingId">Booking ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <returns>Updated booking information</returns>
    [HttpPost("{bookingId}/cancel")]
    // [Authorize] // Commented out temporarily for testing
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDto>> CancelBooking(string bookingId, [FromQuery] string? reason = null)
    {
        try
        {
            if (!Guid.TryParse(bookingId, out var parsedBookingId))
            {
                return BadRequest(new { message = "Booking ID deve ser um GUID válido" });
            }

            var updateDto = new UpdateBookingStatusDto
            {
                Status = "cancelled",
                Notes = "Agendamento cancelado",
                Reason = reason ?? "Cancelamento solicitado"
            };

            var command = new UpdateBookingStatusCommand(parsedBookingId, updateDto);
            var result = await _mediator.Send(command);

            _logger.LogInformation("Booking {BookingId} cancelled with reason: {Reason}", bookingId, reason);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Error cancelling booking {BookingId}: {Message}", bookingId, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid operation cancelling booking {BookingId}: {Message}", bookingId, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
} 