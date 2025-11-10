using System;
using System.Linq;
using System.Threading.Tasks;
using Accounting.Application.DTOs;
using Accounting.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Application.Services
{
    public class DashboardService
    {
        private readonly AccountingDbContext _db;

        public DashboardService(AccountingDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            // Tổng KH & NCC (chưa xóa)
            var totalCustomers = await _db.KhachHang
                .Where(x => !x.DaXoa)
                .CountAsync();

            var totalSuppliers = await _db.NhaCungCap
                .Where(x => !x.DaXoa)
                .CountAsync();

            // Công nợ phải thu (HĐ bán còn nợ)
            var openArAmount = await _db.HoaDonBan
                .Where(x => !x.DaXoa && x.TrangThaiCongNo != "da_thanh_toan")
                .SumAsync(x =>
                    (decimal?)x.TongTien - (decimal?)x.SoTienDaThanhToan ?? 0m);

            // Công nợ phải trả (HĐ mua còn nợ)
            var openApAmount = await _db.HoaDonMua
                .Where(x => x.TrangThaiCongNo != "da_thanh_toan")
                .SumAsync(x =>
                    (decimal?)x.TongTien - (decimal?)x.SoTienDaThanhToan ?? 0m);

            // Doanh thu hôm nay
            var todaySalesAmount = await _db.HoaDonBan
                .Where(x => !x.DaXoa && x.NgayHoaDon.Date == today)
                .SumAsync(x => (decimal?)x.TongTien ?? 0m);

            // Doanh thu tháng hiện tại
            var monthSalesAmount = await _db.HoaDonBan
                .Where(x => !x.DaXoa
                            && x.NgayHoaDon >= monthStart
                            && x.NgayHoaDon < monthEnd)
                .SumAsync(x => (decimal?)x.TongTien ?? 0m);

            return new DashboardSummaryDto
            {
                TotalCustomers = totalCustomers,
                TotalSuppliers = totalSuppliers,
                OpenArAmount = openArAmount,
                OpenApAmount = openApAmount,
                TodaySalesAmount = todaySalesAmount,
                MonthSalesAmount = monthSalesAmount
            };
        }
    }
}
