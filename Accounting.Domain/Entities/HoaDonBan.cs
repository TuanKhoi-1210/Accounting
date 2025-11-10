namespace Accounting.Domain.Entities;


public class HoaDonBan
{
    public long Id { get; set; }
    public string SoHoaDon { get; set; } = "";
    public DateTime NgayHoaDon { get; set; } = DateTime.UtcNow;

    public decimal TienHang { get; set; }
    public decimal TienThue { get; set; }
    public decimal TongTien { get; set; }

    public decimal SoTienDaThanhToan { get; set; } = 0m;
    public string TrangThaiCongNo { get; set; } = "chua_tt";

    public long DonBanId { get; set; }

    public string TrangThai { get; set; } = "draft"; // draft|issued|canceled|paid

    public DateTime? NgayTao { get; set; }
    public string? NguoiTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public string? NguoiCapNhat { get; set; }
    public bool DaXoa { get; set; }

    public List<HoaDonBanDong> Dongs { get; set; } = new();

    // (tuỳ chọn) điều hướng ngược tới phiếu thu
    public List<PhieuThu> PhieuThus { get; set; } = new();
}

public class HoaDonBanDong
{
    public long Id { get; set; }
    public long HoaDonBanId { get; set; }

    // Không còn VatTuId — đây là sản phẩm hoặc dịch vụ in ấn
    public string TenHang { get; set; } = "";         // Ví dụ: "In hộp sữa Vinamilk 500ml"
    public string? QuyCach { get; set; }              // Mô tả ngắn: "Duplex 350gsm, in offset 4 màu"
    public string? DonViTinh { get; set; }            // vd: bộ, thùng, chiếc
    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }

    public decimal TienHang { get; set; }
    public decimal TienThue { get; set; }
    public decimal ThanhTien { get; set; }

    public decimal ThueSuat { get; set; } // %
    public bool DaXoa { get; set; }
}
