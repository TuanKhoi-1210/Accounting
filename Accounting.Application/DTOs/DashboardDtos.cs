namespace Accounting.Application.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalCustomers { get; set; }
        public int TotalSuppliers { get; set; }

        public decimal OpenArAmount { get; set; }   // Công nợ phải thu
        public decimal OpenApAmount { get; set; }   // Công nợ phải trả

        public decimal TodaySalesAmount { get; set; }  // Doanh thu hôm nay
        public decimal MonthSalesAmount { get; set; }  // Doanh thu tháng hiện tại
    }
}
