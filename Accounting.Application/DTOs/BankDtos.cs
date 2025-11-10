namespace Accounting.Application.DTOs
{
    public class PhieuThuNganHangDto
    {
        public long Id { get; set; }
        public string? SoCt { get; set; }
        public DateTime NgayCt { get; set; }

        public long TaiKhoanNganHangId { get; set; }

        public string NguoiNop { get; set; } = default!;
        public decimal SoTien { get; set; }
        public string? LyDo { get; set; }

        public long? HoaDonBanId { get; set; }
    }

    public class PhieuChiNganHangDto
    {
        public long Id { get; set; }
        public string? SoCt { get; set; }
        public DateTime NgayCt { get; set; }

        public long TaiKhoanNganHangId { get; set; }

        public string NguoiNhan { get; set; } = default!;
        public decimal SoTien { get; set; }
        public string? LyDo { get; set; }

        public long? HoaDonMuaId { get; set; }
    }

    // Dòng sổ phụ ngân hàng
    public class SoNganHangItemDto
    {
        public DateTime NgayCt { get; set; }
        public string SoCt { get; set; } = default!;
        public string LoaiPhieu { get; set; } = default!;   // "UNC Thu" / "UNC Chi"
        public string? NoiDung { get; set; }
        public decimal Thu { get; set; }
        public decimal Chi { get; set; }
    }
    public class HoaDonBanNoDto
    {
        public long Id { get; set; }
        public string SoCt { get; set; } = "";
        public DateTime Ngay { get; set; }
        public string DoiTuong { get; set; } = "";
        public decimal TongTien { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo { get; set; }
    }

    public class HoaDonMuaNoDto
    {
        public long Id { get; set; }
        public string SoCt { get; set; } = "";
        public DateTime Ngay { get; set; }
        public string DoiTuong { get; set; } = "";
        public decimal TongTien { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo { get; set; }
    }
}
