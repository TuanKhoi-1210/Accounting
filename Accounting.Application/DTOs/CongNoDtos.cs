namespace Accounting.Application.Dtos
{
    public class CongNoNccSummaryDto
    {
        public long NhaCungCapId { get; set; }
        public string TenNhaCungCap { get; set; } = "";
        public decimal TongPhaiTra { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo => TongPhaiTra - DaThanhToan;
    }

    public class CongNoKhSummaryDto
    {
        public long KhachHangId { get; set; }
        public string TenKhachHang { get; set; } = "";
        public decimal TongPhaiThu { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo => TongPhaiThu - DaThanhToan;
    }

    public class UpdateThanhToanHoaDonDto
    {
        public long HoaDonId { get; set; }
        public decimal SoTienDaThanhToan { get; set; }
        public DateTime? NgayThanhToan { get; set; }
    }
    public class NhacNoKhDto
    {
        public long HoaDonId { get; set; }
        public long KhachHangId { get; set; }
        public string TenKhachHang { get; set; } = "";
        public string SoHoaDon { get; set; } = "";
        public DateTime NgayHoaDon { get; set; }
        public DateTime? HanThanhToan { get; set; }

        public decimal TongTien { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo => TongTien - DaThanhToan;

        // phục vụ hiển thị trạng thái
        public int SoNgayQuaHan { get; set; }        // >0 nếu quá hạn
        public string TrangThaiHan { get; set; } = "trong_han";  // "trong_han" / "den_han" / "qua_han"
    }

    public class KhachHangDetailDto
    {
        public long Id { get; set; }
        public string Ten { get; set; } = "";
        public string? MaSoThue { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? NguoiLienHe { get; set; }
        public string? DiaChi { get; set; }
    }
    public class NhaCungCapDetailDto
    {
        public long Id { get; set; }
        public string Ten { get; set; } = "";
        public string? MaSoThue { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
    }
}
