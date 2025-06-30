using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BarbeariaSaaS.Application.Features.Dashboard.Queries;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize] // Dashboard requires authentication
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IMediator mediator, ILogger<DashboardController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /*
    /// <summary>
    /// Get schedule overview for a specific date
    /// </summary>
    /// <param name="subdomain">Tenant subdomain</param>
    /// <param name="date">Date in YYYY-MM-DD format (optional, defaults to today)</param>
    /// <returns>Schedule overview with timeline and booking details</returns>
    [HttpGet("{subdomain}/schedule-overview")]
    [ProducesResponseType(typeof(ScheduleOverviewResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ScheduleOverviewResponseDto>> GetScheduleOverview(
        string subdomain,
        [FromQuery] string? date = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(subdomain))
            {
                return BadRequest(new { message = "Subdomain é obrigatório" });
            }

            // Parse date or use today as default
            DateOnly targetDate;
            if (string.IsNullOrWhiteSpace(date))
            {
                targetDate = DateOnly.FromDateTime(DateTime.Today);
            }
            else if (!DateOnly.TryParse(date, out targetDate))
            {
                return BadRequest(new { message = "Data deve estar no formato YYYY-MM-DD" });
            }

            // TODO: Implement GetScheduleOverviewQuery
            var query = new GetScheduleOverviewQuery(subdomain, targetDate);
            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                if (result.Error?.Code == "TENANT_NOT_FOUND")
                    return NotFound(result);
                if (result.Error?.Code == "UNAUTHORIZED")
                    return Forbid();
                return BadRequest(result);
            }

            _logger.LogInformation("Schedule overview retrieved for subdomain {Subdomain} on {Date}", 
                subdomain, targetDate.ToString("yyyy-MM-dd"));

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schedule overview for subdomain {Subdomain} on {Date}", 
                subdomain, date);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
    */

    /// <summary>
    /// Get dashboard statistics for a tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Dashboard statistics including bookings, revenue, clients, etc.</returns>
    [HttpGet("stats/{tenantId}")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DashboardStatsDto>> GetStats(string tenantId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tenantId) || !Guid.TryParse(tenantId, out var parsedTenantId))
            {
                return BadRequest(new { message = "Valid tenant ID is required" });
            }

            // TODO: Add authorization check to ensure user belongs to this tenant
            // var userTenantId = GetUserTenantId(); // Get from JWT claims
            // if (userTenantId != parsedTenantId) return Forbid();

            var query = new GetDashboardStatsQuery(parsedTenantId);
            var result = await _mediator.Send(query);

            _logger.LogInformation("Dashboard stats retrieved for tenant {TenantId}", tenantId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats for tenant: {TenantId}", tenantId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get dashboard statistics (alternative endpoint using JWT tenant claim)
    /// </summary>
    /// <returns>Dashboard statistics for the authenticated user's tenant</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DashboardStatsDto>> GetMyStats()
    {
        try
        {
            // Get tenant ID from JWT claims
            var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
            
            if (string.IsNullOrWhiteSpace(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                return BadRequest(new { message = "User is not associated with a tenant" });
            }

            var query = new GetDashboardStatsQuery(tenantId);
            var result = await _mediator.Send(query);

            _logger.LogInformation("Dashboard stats retrieved for authenticated user's tenant {TenantId}", tenantId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats for authenticated user");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get bookings for a tenant (with filters)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="startDate">Start date filter (YYYY-MM-DD)</param>
    /// <param name="endDate">End date filter (YYYY-MM-DD)</param>
    /// <param name="status">Status filter (confirmed, cancelled, completed)</param>
    /// <returns>List of bookings</returns>
    [HttpGet("bookings/{tenantId}")]
    [ProducesResponseType(typeof(IEnumerable<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookings(
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

            _logger.LogInformation("Retrieved {Count} bookings for tenant {TenantId}", result.Count(), tenantId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for tenant: {TenantId}", tenantId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get today's bookings for authenticated user's tenant
    /// </summary>
    /// <returns>List of today's bookings</returns>
    [HttpGet("bookings/today")]
    [ProducesResponseType(typeof(IEnumerable<BookingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetTodayBookings()
    {
        try
        {
            // Get tenant ID from JWT claims
            var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
            
            if (string.IsNullOrWhiteSpace(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                return BadRequest(new { message = "User is not associated with a tenant" });
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var query = new BarbeariaSaaS.Application.Features.Bookings.Queries.GetBookingsQuery(
                tenantId, today, today);
            
            var result = await _mediator.Send(query);

            _logger.LogInformation("Retrieved {Count} bookings for today for tenant {TenantId}", result.Count(), tenantId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's bookings for authenticated user");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Health check for dashboard service
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult Health()
    {
        return Ok(new
        {
            service = "Dashboard",
            status = "Healthy",
            timestamp = DateTime.UtcNow
        });
    }
} 