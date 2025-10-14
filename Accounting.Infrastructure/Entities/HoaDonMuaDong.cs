namespace Accounting.Infrastructure;
public class HoaDonMuaDong
{
    public long Id { get; set; }
    public long HoaDonMuaId { get; set; }
    public long VatTuId { get; set; }
    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public long? ThueSuatId { get; set; }
    public decimal TienThue { get; set; }
    public decimal ThanhTien { get; set; }
}
