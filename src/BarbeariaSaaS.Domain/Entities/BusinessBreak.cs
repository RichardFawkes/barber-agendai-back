using System.ComponentModel.DataAnnotations;

namespace BarbeariaSaaS.Domain.Entities;

public class BusinessBreak
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    [Required]
    public TimeSpan StartTime { get; set; }
    
    [Required]
    public TimeSpan EndTime { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public bool AppliesToAllDays { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
} 