namespace Accounting.Domain.Entities
{
    public class LenhSanXuatDong
    {
        public long Id { get; set; }

        public long LenhId { get; set; }                      // FK -> LenhSanXuat.Id
        public LenhSanXuat? Lenh { get; set; }

        public long VatTuId { get; set; }                     // VT đầu vào (xuat) hoặc TP đầu ra (nhap)
        public VatTu? VatTu { get; set; }

        public string LoaiDong { get; set; } = "xuat";        // 'xuat' | 'nhap'
        public decimal? HeSo { get; set; }                    // định mức cho 1 TP (khi 'xuat')
        public decimal? SoLuong { get; set; }                 // SL xuất/nhập thực tế (nếu có)
        public decimal? DonGia { get; set; }                  // tuỳ chọn cho giá trị
        public decimal? GiaTri { get; set; }                  // tuỳ chọn cho giá trị

        public string? GhiChu { get; set; }
        public bool DaXoa { get; set; }
    }
}
