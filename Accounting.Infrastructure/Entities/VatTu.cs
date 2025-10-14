namespace Accounting.Infrastructure;
public class VatTu
{
    public long Id { get; set; }
    public string Ma { get; set; } = "";
    public string Ten { get; set; } = "";
    public string? KichThuoc { get; set; }
    public string? LoaiGiay { get; set; }
    public int? DinhLuongGsm { get; set; }
    public string? MauIn { get; set; }
    public string? GiaCong { get; set; }
    public long? DonViTinhId { get; set; }
    public DateTime NgayTao { get; set; }
    public string? NguoiTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public string? NguoiCapNhat { get; set; }
    public bool DaXoa { get; set; }
}
