namespace BarbeariaSaaS.Shared.DTOs.Response;

public class TenantDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public TenantBrandingDto Branding { get; set; } = new();
    public TenantSettingsDto Settings { get; set; } = new();
}

public class TenantBrandingDto
{
    public BrandingColorsDto Colors { get; set; } = new();
    public BrandingLogoDto? Logo { get; set; }
    public BrandingFontsDto Fonts { get; set; } = new();
}

public class BrandingColorsDto
{
    public string Primary { get; set; } = "#3B82F6";
    public string Accent { get; set; } = "#10B981";
    public string Secondary { get; set; } = "#6B7280";
    public string Background { get; set; } = "#FFFFFF";
    public string Text { get; set; } = "#1F2937";
    public string Muted { get; set; } = "#9CA3AF";
}

public class BrandingLogoDto
{
    public string Url { get; set; } = string.Empty;
    public string Alt { get; set; } = string.Empty;
}

public class BrandingFontsDto
{
    public string Primary { get; set; } = "Inter";
    public string Secondary { get; set; } = "Inter";
}

public class TenantSettingsDto
{
    public List<BusinessHourDto> BusinessHours { get; set; } = new();
    public BookingSettingsDto Booking { get; set; } = new();
}

public class BusinessHourDto
{
    public string Id { get; set; } = string.Empty;
    public int DayOfWeek { get; set; }
    public bool IsOpen { get; set; }
    public string OpenTime { get; set; } = string.Empty;
    public string CloseTime { get; set; } = string.Empty;
}

public class BookingSettingsDto
{
    public bool AllowOnlineBooking { get; set; } = true;
    public int MaxAdvanceDays { get; set; } = 30;
    public int MinAdvanceHours { get; set; } = 2;
    public bool AllowCancellation { get; set; } = true;
    public int CancellationDeadlineHours { get; set; } = 24;
} 