namespace Accounting.Application.DTOs
{
    public class PhieuThuDto
    {
        public long Id { get; set; }
        public string? SoCt { get; set; }
        public DateTime NgayCt { get; set; }
        public string NguoiNop { get; set; } = default!;
        public decimal SoTien { get; set; }
        public string? LyDo { get; set; }

        public long? HoaDonBanId { get; set; }  // thêm
    }

    public class PhieuChiDto
    {
        public long Id { get; set; }
        public string? SoCt { get; set; }
        public DateTime NgayCt { get; set; }
        public string NguoiNhan { get; set; } = default!;
        public decimal SoTien { get; set; }
        public string? LyDo { get; set; }

        public long? HoaDonMuaId { get; set; }
    }

    public class SoQuyItemDto
    {
        public DateTime NgayCt { get; set; }
        public string SoCt { get; set; } = default!;
        public string LoaiPhieu { get; set; } = default!;  // "Thu tiền" / "Chi tiền"
        public string? NoiDung { get; set; }
        public decimal Thu { get; set; }
        public decimal Chi { get; set; }

        // Số dư sẽ tính bên client; nếu bạn thích có thể thêm ở đây,
        // nhưng API này mình trả Thu/Chi thôi.
    }
    public class KiemKeQuyDto
    {
        public long Id { get; set; }
        public DateTime NgayKk { get; set; }
        public decimal SoDuSo { get; set; }
        public decimal SoDuThucTe { get; set; }
        public decimal ChenhLech { get; set; }
        public string? GhiChu { get; set; }
    }

}