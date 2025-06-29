namespace BarbeariaSaaS.Shared.DTOs.Response;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
    public TenantDto? Tenant { get; set; }
    public DateTime ExpiresAt { get; set; }
} 