using System.ComponentModel.DataAnnotations;

namespace BarbeariaSaaS.Shared.DTOs.Request;

public class UpdateBusinessHoursDto
{
    [Required]
    public List<BusinessHourConfigDto> Schedule { get; set; } = new();
    
    public List<BusinessBreakRequestDto> Breaks { get; set; } = new();
    
    [Required]
    public TenantSettingsRequestDto Settings { get; set; } = new();
}

public class BusinessHourConfigDto
{
    [Required]
    [Range(0, 6, ErrorMessage = "DayOfWeek deve estar entre 0 (Domingo) e 6 (Sábado)")]
    public int DayOfWeek { get; set; }
    
    [Required]
    public string DayName { get; set; } = string.Empty;
    
    public bool IsOpen { get; set; }
    
    public string? StartTime { get; set; }
    
    public string? EndTime { get; set; }
}

public class BusinessBreakRequestDto
{
    [Required]
    public string StartTime { get; set; } = string.Empty;
    
    [Required]
    public string EndTime { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public bool AppliesToAllDays { get; set; } = true;
}

public class TenantSettingsRequestDto
{
    [Range(15, 240, ErrorMessage = "Duração do slot deve estar entre 15 e 240 minutos")]
    public int SlotDurationMinutes { get; set; } = 30;
    
    [Range(1, 365, ErrorMessage = "Dias de antecedência deve estar entre 1 e 365")]
    public int AdvanceBookingDays { get; set; } = 30;
    
    [Range(1, 200, ErrorMessage = "Máximo de agendamentos por dia deve estar entre 1 e 200")]
    public int MaxBookingsPerDay { get; set; } = 50;
    
    [Range(0, 60, ErrorMessage = "Buffer entre agendamentos deve estar entre 0 e 60 minutos")]
    public int BookingBufferMinutes { get; set; } = 0;
    
    [StringLength(50)]
    public string Timezone { get; set; } = "America/Sao_Paulo";
    
    public bool AutoConfirmBookings { get; set; } = true;
}

public class CreateSpecialDaysDto
{
    [Required]
    public List<SpecialDayRequestDto> SpecialDays { get; set; } = new();
}

public class SpecialDayRequestDto
{
    [Required]
    public string Date { get; set; } = string.Empty;
    
    [Required]
    public string Type { get; set; } = string.Empty; // HOLIDAY, SPECIAL_HOURS, CLOSED
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public bool IsOpen { get; set; } = false;
    
    public string? CustomStartTime { get; set; }
    
    public string? CustomEndTime { get; set; }
}

public class CreateManualBlocksDto
{
    [Required]
    public List<ManualBlockRequestDto> Blocks { get; set; } = new();
}

public class ManualBlockRequestDto
{
    [Required]
    public string Date { get; set; } = string.Empty;
    
    [Required]
    public string Type { get; set; } = string.Empty; // TEMPORARY_BLOCK, FULL_DAY_BLOCK
    
    public string? StartTime { get; set; }
    
    public string? EndTime { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Reason { get; set; } = string.Empty;
} 