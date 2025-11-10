namespace Accounting.Domain.Entities;

public class KiemKeQuy
{
    public long Id { get; set; }
    public DateTime NgayKk { get; set; }          // ngày kiểm kê
    public decimal SoDuSo { get; set; }           // số dư theo sổ
    public decimal SoDuThucTe { get; set; }       // số dư thực tế
    public decimal ChenhLech { get; set; }        // SoDuThucTe - SoDuSo
    public string? GhiChu { get; set; }

    public DateTime? NgayTao { get; set; }
    public string? NguoiTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public string? NguoiCapNhat { get; set; }
    public bool DaXoa { get; set; }
}
