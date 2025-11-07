namespace Accounting.Domain.Entities
{
    public class LenhSanXuat
    {
        public long Id { get; set; }
        public string Ma { get; set; } = default!;      // MO-YYYYMMDD-xxx
        public DateTime? NgayLenh { get; set; }

        // Thành phẩm chính (nếu có) – giữ nguyên để nhập kho TP sau này
        public long SanPhamId { get; set; }
        public VatTu? SanPham { get; set; }

        public decimal SoLuongKeHoach { get; set; }
        public decimal? SoLuongThucTe { get; set; }

        // ===== Thông tin NGÀNH IN =====
        public string? TenKhachHang { get; set; }
        public string? TenBaiIn { get; set; }
        public string? TenGiayIn { get; set; }
        public string? KhoIn { get; set; }
        public int? SoMauIn { get; set; }
        public string? HinhThucIn { get; set; }         // offset/flexo/…
        public int? SoCon { get; set; }                 // number-up
        public string? MayIn { get; set; }
        public DateTime? NgayIn { get; set; }
        public decimal? SoLuongThanhPham { get; set; }  // TP hoàn thành

        // ===== Meta =====
        public string? TrangThai { get; set; }          // pending/processing/done/canceled
        public string? GhiChu { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }
        public bool DaXoa { get; set; }

        public ICollection<LenhSanXuatDong> Dongs { get; set; } = new List<LenhSanXuatDong>();
    }
}
