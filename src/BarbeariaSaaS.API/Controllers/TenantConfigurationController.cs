using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BarbeariaSaaS.Shared.DTOs.Request;
using BarbeariaSaaS.Shared.DTOs.Response;
using BarbeariaSaaS.Application.Features.Tenants.Commands;

namespace BarbeariaSaaS.API.Controllers;

[ApiController]
[Route("api/tenant")]
[Produces("application/json")]
// [Authorize] // Temporariamente removido para testes
public class TenantConfigurationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TenantConfigurationController> _logger;

    public TenantConfigurationController(IMediator mediator, ILogger<TenantConfigurationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Update business hours and settings for tenant
    /// </summary>
    /// <param name="subdomain">Tenant subdomain</param>
    /// <param name="updateDto">Business hours configuration</param>
    /// <returns>Update result</returns>
    [HttpPut("{subdomain}/business-hours")]
    [ProducesResponseType(typeof(BusinessHoursResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> UpdateBusinessHours(string subdomain, [FromBody] UpdateBusinessHoursDto updateDto)
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

            _logger.LogInformation("Attempting to update business hours for subdomain: {Subdomain}", subdomain);
            _logger.LogInformation("UpdateDto received with {ScheduleCount} schedule items", updateDto.Schedule?.Count ?? 0);

            var command = new UpdateBusinessHoursCommand(subdomain, updateDto);
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                _logger.LogWarning("Business hours update failed: {ErrorCode} - {ErrorMessage}", 
                    result.Error?.Code, result.Error?.Message);
                
                if (result.Error?.Code == "TENANT_NOT_FOUND")
                    return NotFound(result);
                if (result.Error?.Code == "UNAUTHORIZED")
                    return Forbid();
                return BadRequest(result);
            }

            _logger.LogInformation("Business hours updated successfully for subdomain {Subdomain}", subdomain);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UpdateBusinessHours for subdomain {Subdomain}. Message: {Message}. StackTrace: {StackTrace}", 
                subdomain, ex.Message, ex.StackTrace);
            
            return StatusCode(500, new { 
                message = "Erro interno do servidor", 
                details = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    /// <summary>
    /// Create special days for tenant
    /// </summary>
    /// <param name="subdomain">Tenant subdomain</param>
    /// <param name="createDto">Special days configuration</param>
    /// <returns>Creation result</returns>
    [HttpPost("{subdomain}/special-days")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> CreateSpecialDays(string subdomain, [FromBody] CreateSpecialDaysDto createDto)
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

            // TODO: Implement CreateSpecialDaysCommand
            return CreatedAtAction(nameof(CreateSpecialDays), new { subdomain }, 
                new { success = true, message = "Funcionalidade em desenvolvimento" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating special days for subdomain {Subdomain}", subdomain);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Create manual blocks for tenant
    /// </summary>
    /// <param name="subdomain">Tenant subdomain</param>
    /// <param name="createDto">Manual blocks configuration</param>
    /// <returns>Creation result</returns>
    [HttpPost("{subdomain}/manual-blocks")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> CreateManualBlocks(string subdomain, [FromBody] CreateManualBlocksDto createDto)
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

            // TODO: Implement CreateManualBlocksCommand
            return CreatedAtAction(nameof(CreateManualBlocks), new { subdomain }, 
                new { success = true, message = "Funcionalidade em desenvolvimento" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating manual blocks for subdomain {Subdomain}", subdomain);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Get manual blocks for a date range
    /// </summary>
    /// <param name="subdomain">Tenant subdomain</param>
    /// <param name="startDate">Start date (YYYY-MM-DD)</param>
    /// <param name="endDate">End date (YYYY-MM-DD)</param>
    /// <returns>List of manual blocks</returns>
    [HttpGet("{subdomain}/manual-blocks")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetManualBlocks(
        string subdomain,
        [FromQuery] string startDate,
        [FromQuery] string endDate)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(subdomain))
            {
                return BadRequest(new { message = "Subdomain é obrigatório" });
            }

            if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
            {
                return BadRequest(new { message = "Datas de início e fim são obrigatórias" });
            }

            if (!DateOnly.TryParse(startDate, out var parsedStartDate) || 
                !DateOnly.TryParse(endDate, out var parsedEndDate))
            {
                return BadRequest(new { message = "Datas devem estar no formato YYYY-MM-DD" });
            }

            // TODO: Implement GetManualBlocksQuery
            return Ok(new { success = true, data = new List<object>(), message = "Funcionalidade em desenvolvimento" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting manual blocks for subdomain {Subdomain}", subdomain);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Delete a manual block
    /// </summary>
    /// <param name="subdomain">Tenant subdomain</param>
    /// <param name="blockId">Block ID</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{subdomain}/manual-blocks/{blockId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteManualBlock(string subdomain, string blockId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(subdomain))
            {
                return BadRequest(new { message = "Subdomain é obrigatório" });
            }

            if (!Guid.TryParse(blockId, out var parsedBlockId))
            {
                return BadRequest(new { message = "Block ID deve ser um GUID válido" });
            }

            // TODO: Implement DeleteManualBlockCommand
            _logger.LogInformation("Manual block {BlockId} deletion requested for subdomain {Subdomain}", blockId, subdomain);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting manual block {BlockId} for subdomain {Subdomain}", blockId, subdomain);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
} 