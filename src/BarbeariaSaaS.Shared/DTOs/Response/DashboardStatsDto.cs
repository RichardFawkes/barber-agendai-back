namespace BarbeariaSaaS.Shared.DTOs.Response;

public class DashboardStatsDto
{
    public int TotalBookings { get; set; }
    public decimal TodayRevenue { get; set; }
    public int TotalClients { get; set; }
    public decimal AverageRating { get; set; }
    public int PendingBookings { get; set; }
    public int ConfirmedBookings { get; set; }
} 