namespace BarbeariaSaaS.Shared.DTOs.Response;

public class CreateTenantResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public CreateTenantDataDto? Data { get; set; }
    public Dictionary<string, List<string>>? Errors { get; set; }
}

public class CreateTenantDataDto
{
    public TenantResponseDto Tenant { get; set; } = new();
    public AdminResponseDto Admin { get; set; } = new();
    public AuthResponseDto Auth { get; set; } = new();
    public UrlsResponseDto Urls { get; set; } = new();
}

public class TenantResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Website { get; set; }
    public TenantBrandingResponseDto Branding { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TenantBrandingResponseDto
{
    public BrandingColorsResponseDto Colors { get; set; } = new();
    public BrandingLogoResponseDto? Logo { get; set; }
}

public class BrandingColorsResponseDto
{
    public string Primary { get; set; } = string.Empty;
    public string Accent { get; set; } = string.Empty;
}

public class BrandingLogoResponseDto
{
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

public class AdminResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string ExpiresIn { get; set; } = string.Empty;
}

public class UrlsResponseDto
{
    public string Public { get; set; } = string.Empty;
    public string Admin { get; set; } = string.Empty;
} 