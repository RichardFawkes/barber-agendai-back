using System.ComponentModel.DataAnnotations;

namespace BarbeariaSaaS.Domain.Entities;

public class TenantSetting
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    public int SlotDurationMinutes { get; set; } = 30;
    
    public int AdvanceBookingDays { get; set; } = 30;
    
    public int MaxBookingsPerDay { get; set; } = 50;
    
    public int BookingBufferMinutes { get; set; } = 0;
    
    [StringLength(50)]
    public string Timezone { get; set; } = "America/Sao_Paulo";
    
    public bool AutoConfirmBookings { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
} 