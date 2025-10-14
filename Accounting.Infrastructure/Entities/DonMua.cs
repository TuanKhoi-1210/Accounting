namespace Accounting.Infrastructure;
public class DonMua
{
    public long Id { get; set; }
    public string SoCt { get; set; } = "";             // số chứng từ
    public long NhaCungCapId { get; set; }
    public DateTime NgayDon { get; set; }
    public string TienTe { get; set; } = "VND";
    public decimal TyGia { get; set; } = 1.0000m;
    public bool CoHopDongLon { get; set; }             // >20 triệu
    public string? GhiChu { get; set; }
    public string TrangThai { get; set; } = "nhap";    // nhap/duyet/...
    public decimal TienHang { get; set; }
    public decimal TienThue { get; set; }
    public decimal TongTien { get; set; }
}
