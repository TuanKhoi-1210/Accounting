namespace Accounting.Infrastructure;
public class Kho
{
    public long Id { get; set; }
    public string Ma { get; set; } = "";
    public string Ten { get; set; } = "";
    public DateTime NgayTao { get; set; }
    public string? NguoiTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public string? NguoiCapNhat { get; set; }
    public bool DaXoa { get; set; }
}
