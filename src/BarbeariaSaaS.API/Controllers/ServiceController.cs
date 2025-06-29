using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BarbeariaSaaS.Application.Features.Services.Queries;
using BarbeariaSaaS.Shared.DTOs.Response;
using BarbeariaSaaS.Application.Interfaces;

namespace BarbeariaSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ServiceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ServiceController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ServiceController(IMediator mediator, ILogger<ServiceController> logger, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _logger = logger;
        _unitOfWork = unitOfWork;
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
    /// Create a new service (admin only)
    /// </summary>
    /// <param name="createServiceDto">Service data</param>
    /// <returns>Created service</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceDto>> CreateService([FromBody] BarbeariaSaaS.Shared.DTOs.Request.CreateServiceDto createServiceDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new BarbeariaSaaS.Application.Features.Services.Commands.CreateServiceCommand(createServiceDto);
            var result = await _mediator.Send(command);

            _logger.LogInformation("Service created successfully: {ServiceName} for tenant {TenantId}", 
                createServiceDto.Name, createServiceDto.TenantId);

            return CreatedAtAction(nameof(CreateService), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service: {ServiceName}", createServiceDto.Name);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get all services for a tenant (admin)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>List of all services (active and inactive)</returns>
    [HttpGet("tenant/{tenantId}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ServiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ServiceDto>>> GetAllServices(string tenantId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tenantId) || !Guid.TryParse(tenantId, out var parsedTenantId))
            {
                return BadRequest(new { message = "Valid tenant ID is required" });
            }

            // Para admin, mostrar todos os serviços (ativos e inativos)
            var services = await _unitOfWork.Services.GetByTenantIdAsync(parsedTenantId);
            
            var result = services.Select(s => new ServiceDto
            {
                Id = s.Id.ToString(),
                Name = s.Name,
                Description = s.Description,
                Price = s.Price,
                DurationMinutes = s.DurationMinutes,
                Color = s.Color,
                IsActive = s.IsActive
            });

            _logger.LogInformation("Retrieved {Count} services for tenant {TenantId}", result.Count(), tenantId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting services for tenant: {TenantId}", tenantId);
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