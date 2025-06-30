namespace BarbeariaSaaS.Shared.DTOs.Response;

public class AvailableDatesResponseDto
{
    public bool Success { get; set; }
    public AvailableDatesDataDto? Data { get; set; }
    public ErrorDto? Error { get; set; }
}

public class AvailableDatesDataDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string ServiceId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int ServiceDuration { get; set; }
    public List<AvailableDateDto> AvailableDates { get; set; } = new();
    public AvailabilityMonthSummaryDto Summary { get; set; } = new();
}

public class AvailableDateDto
{
    public string Date { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
    public bool HasAvailability { get; set; }
    public int TotalSlots { get; set; }
    public int AvailableSlots { get; set; }
    public int BookedSlots { get; set; }
    public string? FirstAvailableTime { get; set; }
    public string? LastAvailableTime { get; set; }
    public string? Reason { get; set; }
    public string? ReasonDetail { get; set; }
}

public class AvailabilityMonthSummaryDto
{
    public int TotalDaysInMonth { get; set; }
    public int AvailableDays { get; set; }
    public int ClosedDays { get; set; }
    public int TotalAvailableSlots { get; set; }
    public int TotalBookedSlots { get; set; }
    public decimal OccupationRate { get; set; }
}

public class AvailableTimesResponseDto
{
    public bool Success { get; set; }
    public AvailableTimesDataDto? Data { get; set; }
    public ErrorDto? Error { get; set; }
}

public class AvailableTimesDataDto
{
    public string Date { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int ServiceDuration { get; set; }
    public List<string> AvailableTimes { get; set; } = new();
    public BusinessHoursDto BusinessHours { get; set; } = new();
    public List<BusinessBreakDto> Breaks { get; set; } = new();
    public List<OccupiedSlotDto> OccupiedSlots { get; set; } = new();
    public List<BlockedSlotDto> BlockedSlots { get; set; } = new();
    public AvailabilityDaySummaryDto Summary { get; set; } = new();
}

public class BusinessHoursDto
{
    public bool IsOpen { get; set; }
    public string? Start { get; set; }
    public string? End { get; set; }
    public int DayOfWeek { get; set; }
}

public class BusinessBreakDto
{
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class OccupiedSlotDto
{
    public string Time { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string BookingId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class BlockedSlotDto
{
    public string Time { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}

public class AvailabilityDaySummaryDto
{
    public int TotalSlots { get; set; }
    public int AvailableSlots { get; set; }
    public int OccupiedSlots { get; set; }
    public int BlockedSlots { get; set; }
}

public class ErrorDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }
} 