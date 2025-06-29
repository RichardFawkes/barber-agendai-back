using System.ComponentModel.DataAnnotations;

namespace BarbeariaSaaS.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? Phone { get; set; }
    
    public UserRole Role { get; set; }
    public Guid? TenantId { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Tenant? Tenant { get; set; }
}

public enum UserRole
{
    SuperAdmin = 1,
    TenantAdmin = 2,
    TenantEmployee = 3,
    Customer = 4
} 