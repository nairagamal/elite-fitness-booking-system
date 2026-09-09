namespace elite.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveMemberships { get; set; }
        public int TotalBookingsToday { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int LowStockProducts { get; set; }
    }

    public class RevenueReportDto
    {
        public DateTime Date { get; set; }
        public decimal MembershipRevenue { get; set; }
        public decimal ProductRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public int BookingsCount { get; set; }
    }
}
