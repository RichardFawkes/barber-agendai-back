using System.ComponentModel.DataAnnotations;

namespace BarbeariaSaaS.Shared.DTOs.Request;

public class CreateTenantWithAdminDto
{
    [Required]
    public TenantDataDto Tenant { get; set; } = new();
    
    [Required]
    public AdminDataDto Admin { get; set; } = new();
}

public class TenantDataDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-z0-9][a-z0-9-]*[a-z0-9]$", 
        ErrorMessage = "Subdomain must contain only lowercase letters, numbers and hyphens")]
    public string Subdomain { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Address { get; set; } = string.Empty;
    
    public string? Website { get; set; }
    
    public LogoDataDto? Logo { get; set; }
    
    public BrandingDataDto? Branding { get; set; }
}

public class AdminDataDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}

public class LogoDataDto
{
    [Url]
    public string? Url { get; set; }
    
    public string? FileName { get; set; }
}

public class BrandingDataDto
{
    public ColorsDataDto Colors { get; set; } = new();
}

public class ColorsDataDto
{
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Must be a valid hex color")]
    public string Primary { get; set; } = "#0f172a";
    
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Must be a valid hex color")]
    public string Accent { get; set; } = "#fbbf24";
} 