using System.ComponentModel.DataAnnotations;

namespace BarbeariaSaaS.Shared.DTOs.Request;

public class CreateServiceDto
{
    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, 9999.99)]
    public decimal Price { get; set; }

    [Required]
    [Range(5, 480)] // 5 minutos a 8 horas
    public int DurationMinutes { get; set; }

    [StringLength(7)] // Para formato #RRGGBB
    public string? Color { get; set; }
} 