namespace Accounting.Infrastructure
{
    public class KhachHang
    {
        public long Id { get; set; }
        public string Ma { get; set; } = "";
        public string Ten { get; set; } = "";
        public string? MaSoThue { get; set; }
        public string? DiaChi { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public DateTime NgayTao { get; set; }
        public string? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public string? NguoiCapNhat { get; set; }
        public bool DaXoa { get; set; }
    }
}
