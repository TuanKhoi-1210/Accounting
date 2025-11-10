namespace Accounting.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string TenDangNhap { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string VaiTro { get; set; } = "";
        public bool DangHoatDong { get; set; }
    }

    public class UserEditDto
    {
        public int? Id { get; set; }
        public string TenDangNhap { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string VaiTro { get; set; } = "";
        public bool DangHoatDong { get; set; } = true;
        public string? MatKhauMoi { get; set; }   // nếu null thì giữ nguyên
    }

    public class LoginRequestDto
    {
        public string TenDangNhap { get; set; } = "";
        public string MatKhau { get; set; } = "";
    }

    public class LoginResultDto
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public UserDto? User { get; set; }
    }
}
