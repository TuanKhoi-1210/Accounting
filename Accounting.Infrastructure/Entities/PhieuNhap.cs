namespace Accounting.Infrastructure;
public class PhieuNhap
{
    public long Id { get; set; }
    public string SoCt { get; set; } = "";
    public long? NhaCungCapId { get; set; }
    public long KhoId { get; set; }
    public DateTime NgayNhap { get; set; }
    public long? DonMuaId { get; set; }
    public string? GhiChu { get; set; }
    public DateTime NgayTao { get; set; }
    public string? NguoiTao { get; set; }
}
