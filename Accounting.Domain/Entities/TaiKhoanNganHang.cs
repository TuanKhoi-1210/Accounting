namespace Accounting.Domain.Entities
{
    public class TaiKhoanNganHang
    {
        public long Id { get; set; }
        public string Ma { get; set; } = null!;
        public string TenNganHang { get; set; } = null!;
        public string SoTaiKhoan { get; set; } = null!;
        public string? TienTe { get; set; }
    }
}
