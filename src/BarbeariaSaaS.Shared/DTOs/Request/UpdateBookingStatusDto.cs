using System.ComponentModel.DataAnnotations;

namespace BarbeariaSaaS.Shared.DTOs.Request;

public class UpdateBookingStatusDto
{
    /// <summary>
    /// New status for the booking
    /// Values: pending, confirmed, in_progress, completed, cancelled, no_show
    /// </summary>
    [Required(ErrorMessage = "Status é obrigatório")]
    [StringLength(20, ErrorMessage = "Status deve ter no máximo 20 caracteres")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Optional notes about the status change
    /// </summary>
    [StringLength(500, ErrorMessage = "Observações devem ter no máximo 500 caracteres")]
    public string? Notes { get; set; }

    /// <summary>
    /// Reason for status change (especially for cancellations)
    /// </summary>
    [StringLength(200, ErrorMessage = "Motivo deve ter no máximo 200 caracteres")]
    public string? Reason { get; set; }
} 