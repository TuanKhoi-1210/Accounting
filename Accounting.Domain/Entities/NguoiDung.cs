namespace Accounting.Domain.Entities
{
    public class NguoiDung
    {
        public int Id { get; set; }

        public string TenDangNhap { get; set; } = default!;   // username
        public string HoTen { get; set; } = default!;         // full name

        public string MatKhauHash { get; set; } = default!;   // hash mật khẩu

        /// <summary>
        /// admin, ke_toan, kho, mua_ban
        /// </summary>
        public string VaiTro { get; set; } = default!;

        public bool DangHoatDong { get; set; } = true;

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public string? NguoiTao { get; set; }
        public DateTime? NgaySua { get; set; }
        public string? NguoiSua { get; set; }
    }
}
