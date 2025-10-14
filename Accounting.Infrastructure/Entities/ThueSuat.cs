namespace Accounting.Infrastructure;
public class ThueSuat
{
    public long Id { get; set; }
    public string Ten { get; set; } = "";
    public decimal TyLe { get; set; }    // 0.0000 ~ 1.0000
    public bool DangHoatDong { get; set; } = true;
}
