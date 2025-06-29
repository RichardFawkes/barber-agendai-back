using MediatR;
using Microsoft.AspNetCore.Mvc;
using BarbeariaSaaS.Application.Features.Tenants.Queries;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TenantController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TenantController> _logger;

    public TenantController(IMediator mediator, ILogger<TenantController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get tenant information by subdomain
    /// </summary>
    /// <param name="subdomain">Tenant subdomain</param>
    /// <returns>Tenant configuration and branding</returns>
    [HttpGet("by-subdomain/{subdomain}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TenantDto>> GetBySubdomain(string subdomain)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(subdomain))
            {
                return BadRequest(new { message = "Subdomain is required" });
            }

            var query = new GetTenantBySubdomainQuery(subdomain.ToLower());
            var result = await _mediator.Send(query);

            if (result == null)
            {
                _logger.LogWarning("Tenant not found for subdomain: {Subdomain}", subdomain);
                return NotFound(new { message = "Tenant not found" });
            }

            _logger.LogInformation("Tenant found for subdomain: {Subdomain}", subdomain);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant by subdomain: {Subdomain}", subdomain);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Health check for tenant service
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult Health()
    {
        return Ok(new
        {
            service = "Tenant",
            status = "Healthy",
            timestamp = DateTime.UtcNow
        });
    }
} 