using System.ComponentModel.DataAnnotations;

namespace BarbeariaSaaS.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Subdomain { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [StringLength(200)]
    [EmailAddress]
    public string? Email { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(500)]
    public string? Website { get; set; }
    
    public TenantBranding? Branding { get; set; }
    public TenantSettings? Settings { get; set; }
    
    public TenantStatus Status { get; set; } = TenantStatus.Active;
    public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Basic;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<BusinessHour> BusinessHours { get; set; } = new List<BusinessHour>();
    public ICollection<ServiceCategory> ServiceCategories { get; set; } = new List<ServiceCategory>();
    public ICollection<File> Files { get; set; } = new List<File>();
}

public enum TenantStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    PendingActivation = 4
}

public enum SubscriptionPlan
{
    Basic = 1,
    Professional = 2,
    Enterprise = 3
}

public class TenantBranding
{
    public BrandingColors Colors { get; set; } = new();
    public BrandingLogo? Logo { get; set; }
    public BrandingFonts Fonts { get; set; } = new();
}

public class BrandingColors
{
    public string Primary { get; set; } = "#3B82F6";
    public string Accent { get; set; } = "#10B981";
    public string Secondary { get; set; } = "#6B7280";
    public string Background { get; set; } = "#FFFFFF";
    public string Text { get; set; } = "#1F2937";
    public string Muted { get; set; } = "#9CA3AF";
}

public class BrandingLogo
{
    public string Url { get; set; } = string.Empty;
    public string Alt { get; set; } = string.Empty;
}

public class BrandingFonts
{
    public string Primary { get; set; } = "Inter";
    public string Secondary { get; set; } = "Inter";
}

public class TenantSettings
{
    public List<BusinessHourSettings> BusinessHours { get; set; } = new();
    public BookingSettings Booking { get; set; } = new();
}

public class BusinessHourSettings
{
    public string Id { get; set; } = string.Empty;
    public int DayOfWeek { get; set; }
    public bool IsOpen { get; set; }
    public string OpenTime { get; set; } = string.Empty;
    public string CloseTime { get; set; } = string.Empty;
}

public class BookingSettings
{
    public bool AllowOnlineBooking { get; set; } = true;
    public int MaxAdvanceDays { get; set; } = 30;
    public int MinAdvanceHours { get; set; } = 2;
    public bool AllowCancellation { get; set; } = true;
    public int CancellationDeadlineHours { get; set; } = 24;
} 