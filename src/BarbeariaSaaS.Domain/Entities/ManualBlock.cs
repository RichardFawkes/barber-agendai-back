using System.ComponentModel.DataAnnotations;

namespace BarbeariaSaaS.Domain.Entities;

public class ManualBlock
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    [Required]
    public DateOnly Date { get; set; }
    
    public TimeSpan? StartTime { get; set; }
    
    public TimeSpan? EndTime { get; set; }
    
    [Required]
    public ManualBlockType Type { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Reason { get; set; } = string.Empty;
    
    public Guid? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public User? CreatedByUser { get; set; }
}

public enum ManualBlockType
{
    TemporaryBlock = 1,
    FullDayBlock = 2
} 