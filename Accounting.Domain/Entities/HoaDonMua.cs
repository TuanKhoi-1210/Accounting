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

        public decimal? TienHang { get; set; }
        public decimal? TienThue { get; set; }
        public decimal? TongTien { get; set; }
        public string? TrangThai { get; set; }

        public DateTime? NgayTao { get; set; }
        public string? NguoiTao { get; set; }

        public virtual ICollection<HoaDonMuaDong> Dong { get; set; } = new List<HoaDonMuaDong>();
    }
}
