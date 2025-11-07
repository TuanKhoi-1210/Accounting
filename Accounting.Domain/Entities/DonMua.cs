namespace Accounting.Domain.Entities
{
    public class DonMua
    {
        public long Id { get; set; }
        public string SoCt { get; set; } = null!;
        public long NhaCungCapId { get; set; }
        public DateTime? NgayDon { get; set; }
        public string? TienTe { get; set; }
        public decimal TyGia { get; set; }

        // ➜ Không nullable + mặc định 0
        public decimal TienHang { get; set; } = 0m;
        public decimal TienThue { get; set; } = 0m;
        public decimal TongTien { get; set; } = 0m;

        public bool CoHopDongLon { get; set; }
        public string? GhiChu { get; set; }
        public string TrangThai { get; set; } = "nhap";

        public List<DonMuaDong> Dong { get; set; } = new();
    }

}
