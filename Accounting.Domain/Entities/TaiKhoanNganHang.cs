namespace Accounting.Domain.Entities
{
    public class TaiKhoanNganHang
    {
        public int Id { get; set; }
        public string Ma { get; set; } = null!;
        public string TenNganHang { get; set; } = null!;
        public string SoTaiKhoan { get; set; } = null!;
        public string? TienTe { get; set; }
    }
}
