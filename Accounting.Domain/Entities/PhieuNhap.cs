namespace Accounting.Domain.Entities
{
    public class PhieuNhap
    {
        public int Id { get; set; }
        public string SoCt { get; set; } = null!;
        public int? NhaCungCapId { get; set; }
        public int? KhoId { get; set; }
        public int? DonMuaId { get; set; }

        public DateTime NgayNhap { get; set; }
        public string? GhiChu { get; set; }

        public DateTime? NgayTao { get; set; }
        public string? NguoiTao { get; set; }

        public virtual ICollection<PhieuNhapDong> Dong { get; set; } = new List<PhieuNhapDong>();
    }
}
