namespace Accounting.Infrastructure;
public class HoaDonMua
{
    public long Id { get; set; }
    public string SoCt { get; set; } = "";
    public long NhaCungCapId { get; set; }
    public DateTime NgayHoaDon { get; set; }
    public DateTime? HanThanhToan { get; set; }
    public long? DonMuaId { get; set; }
    public decimal TienHang { get; set; }
    public decimal TienThue { get; set; }
    public decimal TongTien { get; set; }
    public string TrangThai { get; set; } = "con_no"; // con_no/da_thanh_toan
    public DateTime NgayTao { get; set; }
    public string? NguoiTao { get; set; }
}
