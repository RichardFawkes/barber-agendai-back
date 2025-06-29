using MediatR;
using Microsoft.AspNetCore.Mvc;
using BarbeariaSaaS.Application.Features.Tenants.Queries;
using BarbeariaSaaS.Application.Features.Tenants.Commands;
using BarbeariaSaaS.Shared.DTOs.Response;
using BarbeariaSaaS.Shared.DTOs.Request;

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
    /// Create a new tenant (barbershop) with admin user
    /// </summary>
    /// <param name="createTenantDto">Tenant and admin data</param>
    /// <returns>Created tenant with authentication token</returns>
    [HttpPost("create")]
    [ProducesResponseType(typeof(CreateTenantResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(CreateTenantResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(CreateTenantResponseDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(CreateTenantResponseDto), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateTenantResponseDto>> Create([FromBody] CreateTenantWithAdminDto createTenantDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                    );

                var errorResponse = new CreateTenantResponseDto
                {
                    Success = false,
                    Message = "Dados inválidos",
                    Errors = modelErrors
                };

                return BadRequest(errorResponse);
            }

            var command = new CreateTenantWithAdminCommand(createTenantDto);
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                if (result.Message == "Conflito")
                {
                    return Conflict(result);
                }
                
                if (result.Message == "Dados inválidos")
                {
                    return UnprocessableEntity(result);
                }

                return StatusCode(500, result);
            }

            _logger.LogInformation("Tenant created successfully: {Subdomain}", createTenantDto.Tenant.Subdomain);
            return Created($"/api/tenant/by-subdomain/{createTenantDto.Tenant.Subdomain}", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant");
            
            var errorResponse = new CreateTenantResponseDto
            {
                Success = false,
                Message = "Erro interno do servidor"
            };
            
            return StatusCode(500, errorResponse);
        }
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