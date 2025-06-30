namespace BarbeariaSaaS.Shared.DTOs.Response;

public class BusinessHoursResponseDto
{
    public bool Success { get; set; }
    public BusinessHoursDataDto? Data { get; set; }
    public ErrorDto? Error { get; set; }
}

public class BusinessHoursDataDto
{
    public string Message { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public int SavedHours { get; set; }
} 