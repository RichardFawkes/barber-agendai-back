using System.ComponentModel.DataAnnotations;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Shared.DTOs.Request;

public class CreateTenantDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Subdomain { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(200)]
    public string OwnerName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string OwnerEmail { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    public TenantBrandingDto? Branding { get; set; }
    public TenantSettingsDto? Settings { get; set; }
} 