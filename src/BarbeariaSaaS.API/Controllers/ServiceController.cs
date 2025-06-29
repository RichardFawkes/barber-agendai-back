using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BarbeariaSaaS.Application.Features.Services.Queries;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ServiceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ServiceController> _logger;

    public ServiceController(IMediator mediator, ILogger<ServiceController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get active services for a tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>List of active services</returns>
    [HttpGet("tenant/{tenantId}/active")]
    [ProducesResponseType(typeof(IEnumerable<ServiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ServiceDto>>> GetActiveServices(string tenantId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tenantId) || !Guid.TryParse(tenantId, out var parsedTenantId))
            {
                return BadRequest(new { message = "Valid tenant ID is required" });
            }

            var query = new GetActiveServicesQuery(parsedTenantId);
            var result = await _mediator.Send(query);

            _logger.LogInformation("Retrieved {Count} active services for tenant {TenantId}", 
                result.Count(), tenantId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active services for tenant: {TenantId}", tenantId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get active services for a tenant (public endpoint)
    /// </summary>
    /// <param name="subdomain">Tenant subdomain</param>
    /// <returns>List of active services for public booking</returns>
    [HttpGet("public/{subdomain}")]
    [ProducesResponseType(typeof(IEnumerable<ServiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ServiceDto>>> GetPublicServices(string subdomain)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(subdomain))
            {
                return BadRequest(new { message = "Subdomain is required" });
            }

            // First get tenant by subdomain
            var tenantQuery = new BarbeariaSaaS.Application.Features.Tenants.Queries.GetTenantBySubdomainQuery(subdomain.ToLower());
            var tenant = await _mediator.Send(tenantQuery);

            if (tenant == null)
            {
                return NotFound(new { message = "Tenant not found" });
            }

            // Then get services
            var servicesQuery = new GetActiveServicesQuery(Guid.Parse(tenant.Id));
            var result = await _mediator.Send(servicesQuery);

            _logger.LogInformation("Retrieved {Count} public services for subdomain {Subdomain}", 
                result.Count(), subdomain);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting public services for subdomain: {Subdomain}", subdomain);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Health check for service service
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult Health()
    {
        return Ok(new
        {
            service = "Service",
            status = "Healthy",
            timestamp = DateTime.UtcNow
        });
    }
} 