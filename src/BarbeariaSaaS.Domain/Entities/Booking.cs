using System.ComponentModel.DataAnnotations;

namespace BarbeariaSaaS.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    [Required]
    public Guid ServiceId { get; set; }
    
    public Guid? CustomerId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string CustomerPhone { get; set; } = string.Empty;
    
    [Required]
    public DateTime BookingDate { get; set; }
    
    [Required]
    public TimeSpan BookingTime { get; set; }
    
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    [Required]
    public decimal ServicePrice { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public Customer? Customer { get; set; }
}

public enum BookingStatus
{
    Pending = 1,
    Confirmed = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5,
    NoShow = 6
} 