// PhieuChiNganHang.cs
namespace Accounting.Domain.Entities
{
    public class PhieuChiNganHang
    {
        public long Id { get; set; }
        public string SoCt { get; set; } = default!;
        public DateTime NgayCt { get; set; }

        public long TaiKhoanNganHangId { get; set; }

        public string NguoiNhan { get; set; } = default!;
        public decimal SoTien { get; set; }
        public string? LyDo { get; set; }

        public long? HoaDonMuaId { get; set; }

        public DateTime? NgayTao { get; set; }
        public string? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public string? NguoiCapNhat { get; set; }
        public bool DaXoa { get; set; }
    }
}
