namespace Accounting.Domain.Entities;

public class DonBan
{
    public long Id { get; set; }
    public string SoCt { get; set; } = "";
    public DateTime NgayDon { get; set; } = DateTime.UtcNow;

    public long KhachHangId { get; set; }
    public long? KhoId { get; set; }                 // nếu có xuất thành phẩm từ kho thành phẩm
    public string TrangThai { get; set; } = "draft"; // draft|confirmed|in_production|shipped|invoiced|paid|canceled

    public string? GhiChu { get; set; }
    public decimal TienHang { get; set; }
    public decimal TienChietKhau { get; set; }
    public decimal TienThue { get; set; }
    public decimal TongTien { get; set; }

    public DateTime? NgayTao { get; set; }
    public string? NguoiTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public string? NguoiCapNhat { get; set; }
    public bool DaXoa { get; set; }

    public List<DonBanDong> Dongs { get; set; } = new();
}

public class DonBanDong
{
    public long Id { get; set; }
    public long DonBanId { get; set; }

    // KHÁC với “vật tư”: dòng bán là sản phẩm/đơn hàng in
    public long? SanPhamId { get; set; }             // nếu chọn mẫu SP có sẵn
    public string TenHang { get; set; } = "";        // tên job, VD: "Hộp sữa Vinamilk 500ml"
    public string? QuyCach { get; set; }             // mô tả nhanh: "Offset 4 màu, Duplex 350gsm, phủ UV"
    public string? SpecJson { get; set; }            // JSON chi tiết (KT dài/rộng/cao, quy trình, số khuôn, in 2 mặt, cán màng…)

    public decimal SoLuong { get; set; }
    public string? DonViTinh { get; set; }           // thùng/chiếc/bộ…
    public decimal DonGia { get; set; }

    public decimal TienHang { get; set; }
    public decimal TienChietKhau { get; set; }
    public decimal ThueSuat { get; set; }            // %
    public decimal TienThue { get; set; }
    public decimal ThanhTien { get; set; }

    public bool DaXoa { get; set; }
}
