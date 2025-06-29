using System.ComponentModel.DataAnnotations;

namespace BarbeariaSaaS.Domain.Entities;

public class BusinessHour
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    [Required]
    public int DayOfWeek { get; set; } // 0=Sunday, 1=Monday, etc.
    
    public bool IsOpen { get; set; } = true;
    
    [Required]
    public TimeSpan OpenTime { get; set; }
    
    [Required]
    public TimeSpan CloseTime { get; set; }
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
} 