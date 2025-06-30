namespace BarbeariaSaaS.Shared.DTOs.Response;

public class ScheduleOverviewResponseDto
{
    public bool Success { get; set; }
    public ScheduleOverviewDataDto? Data { get; set; }
    public ErrorDto? Error { get; set; }
}

public class ScheduleOverviewDataDto
{
    public string Date { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
    public BusinessHoursDto BusinessHours { get; set; } = new();
    public ScheduleSummaryDto Summary { get; set; } = new();
    public List<TimelineSlotDto> Timeline { get; set; } = new();
    public List<UpcomingBookingDto> UpcomingBookings { get; set; } = new();
}

public class ScheduleSummaryDto
{
    public int TotalSlots { get; set; }
    public int BookedSlots { get; set; }
    public int AvailableSlots { get; set; }
    public int BlockedSlots { get; set; }
    public decimal OccupationRate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageServiceValue { get; set; }
}

public class TimelineSlotDto
{
    public string Time { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string Status { get; set; } = string.Empty;
    public BookingDetailDto? Booking { get; set; }
    public string? Reason { get; set; }
    public string? BlockType { get; set; }
}

public class BookingDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public decimal ServicePrice { get; set; }
    public int ServiceDuration { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpcomingBookingDto
{
    public string Time { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
} 