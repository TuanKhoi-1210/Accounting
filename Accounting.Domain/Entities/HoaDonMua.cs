namespace Accounting.Domain.Entities
{
    public class HoaDonMua
    {
        public int Id { get; set; }
        public string SoCt { get; set; } = null!;
        public int? NhaCungCapId { get; set; }
        public int? DonMuaId { get; set; }

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
