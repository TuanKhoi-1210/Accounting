namespace Accounting.Domain.Entities
{
    public class HoaDonMua
    {
        public long Id { get; set; }
        public string SoCt { get; set; } = null!;
        public long? NhaCungCapId { get; set; }
        public long? DonMuaId { get; set; }

        public DateTime NgayHoaDon { get; set; }
        public DateTime? HanThanhToan { get; set; }

        public decimal SoTienDaThanhToan { get; set; } = 0m;
        public string TrangThaiCongNo { get; set; } = "chua_tt";
        public decimal? TienHang { get; set; }
        public decimal? TienThue { get; set; }
        public decimal? TongTien { get; set; }
        public string? TrangThai { get; set; }

        public DateTime? NgayTao { get; set; }
        public string? NguoiTao { get; set; }
        public List<PhieuChi> PhieuChis { get; set; } = new();
        public virtual ICollection<HoaDonMuaDong> Dong { get; set; } = new List<HoaDonMuaDong>();
    }
}
