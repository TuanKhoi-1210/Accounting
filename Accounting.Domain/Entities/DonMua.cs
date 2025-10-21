namespace Accounting.Domain.Entities
{
    public class DonMua
    {
        public int Id { get; set; }
        public string SoCt { get; set; } = null!;
        public int? NhaCungCapId { get; set; }
        public DateTime NgayDon { get; set; }

        public string? TienTe { get; set; }
        public decimal TyGia { get; set; } = 1.0000m;
        public bool CoHopDongLon { get; set; }
        public string? GhiChu { get; set; }
        public string? TrangThai { get; set; }

        public decimal? TienHang { get; set; }
        public decimal? TienThue { get; set; }
        public decimal? TongTien { get; set; }

        public virtual ICollection<DonMuaDong> Dong { get; set; } = new List<DonMuaDong>();
    }
}
