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