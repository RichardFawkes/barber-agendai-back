using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BarbeariaSaaS.Application.Features.Bookings.Commands;
using BarbeariaSaaS.Shared.DTOs.Request;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BookingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BookingController> _logger;

    public BookingController(IMediator mediator, ILogger<BookingController> logger)
    {
        _mediator = mediator;
        _logger = logger;
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
                return BadRequest(new { message = "Subdomain is required" });
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
                return NotFound(new { message = "Tenant not found" });
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
    /// Get available time slots for booking
    /// </summary>
    /// <param name="subdomain">Tenant subdomain</param>
    /// <param name="serviceId">Service ID</param>
    /// <param name="date">Date in YYYY-MM-DD format</param>
    /// <returns>List of available time slots</returns>
    [HttpGet("available-times/{subdomain}")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<string>>> GetAvailableTimes(
        string subdomain, 
        [FromQuery] string serviceId, 
        [FromQuery] string date)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(subdomain))
            {
                return BadRequest(new { message = "Subdomain is required" });
            }

            if (!Guid.TryParse(serviceId, out var parsedServiceId))
            {
                return BadRequest(new { message = "Valid service ID is required" });
            }

            if (!DateOnly.TryParse(date, out var parsedDate))
            {
                return BadRequest(new { message = "Valid date is required (YYYY-MM-DD format)" });
            }

            // Get tenant by subdomain first
            var tenantQuery = new BarbeariaSaaS.Application.Features.Tenants.Queries.GetTenantBySubdomainQuery(subdomain.ToLower());
            var tenant = await _mediator.Send(tenantQuery);

            if (tenant == null)
            {
                return NotFound(new { message = "Tenant not found" });
            }

            var query = new BarbeariaSaaS.Application.Features.Bookings.Queries.GetAvailableTimesQuery(
                Guid.Parse(tenant.Id), 
                parsedServiceId, 
                parsedDate);
            
            var result = await _mediator.Send(query);

            // Convert TimeOnly to string format
            var timeStrings = result.Select(t => t.ToString("HH:mm"));

            _logger.LogInformation("Retrieved {Count} available times for subdomain {Subdomain} on {Date}", 
                timeStrings.Count(), subdomain, date);

            return Ok(timeStrings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available times for subdomain {Subdomain} on {Date}", subdomain, date);
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
} 