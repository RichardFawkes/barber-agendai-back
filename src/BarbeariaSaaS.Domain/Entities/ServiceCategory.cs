using System.ComponentModel.DataAnnotations;

namespace BarbeariaSaaS.Domain.Entities;

public class ServiceCategory
{
    public int Id { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(300)]
    public string? Description { get; set; }
    
    [StringLength(7)]
    public string? Color { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Service> Services { get; set; } = new List<Service>();
} 