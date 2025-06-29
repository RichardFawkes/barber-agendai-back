using System.ComponentModel.DataAnnotations;

namespace BarbeariaSaaS.Shared.DTOs.Request;

public class CreateBookingDto
{
    [Required]
    public string TenantId { get; set; } = string.Empty;
    
    [Required]
    public string ServiceId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string CustomerEmail { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string CustomerPhone { get; set; } = string.Empty;
    
    [Required]
    public string Date { get; set; } = string.Empty; // ISO format
    
    [Required]
    public string Time { get; set; } = string.Empty; // HH:mm format
    
    [StringLength(1000)]
    public string? Notes { get; set; }
} 