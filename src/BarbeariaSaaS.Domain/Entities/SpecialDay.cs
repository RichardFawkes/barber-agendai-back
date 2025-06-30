using System.ComponentModel.DataAnnotations;

namespace BarbeariaSaaS.Domain.Entities;

public class SpecialDay
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    [Required]
    public DateOnly Date { get; set; }
    
    [Required]
    public SpecialDayType Type { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public bool IsOpen { get; set; } = false;
    
    public TimeSpan? CustomStartTime { get; set; }
    
    public TimeSpan? CustomEndTime { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
}

public enum SpecialDayType
{
    Holiday = 1,
    SpecialHours = 2,
    Closed = 3
} 