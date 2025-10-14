namespace Accounting.Infrastructure;
public class DonMuaDong
{
    public long Id { get; set; }
    public long DonMuaId { get; set; }
    public long VatTuId { get; set; }
    public string? KichThuoc { get; set; }
    public string? LoaiGiay { get; set; }
    public int? DinhLuongGsm { get; set; }
    public string? MauIn { get; set; }
    public string? GiaCong { get; set; }
    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public long? ThueSuatId { get; set; }
    public decimal TienThue { get; set; }
    public decimal ThanhTien { get; set; }
}
