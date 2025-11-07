using System;

namespace Accounting.Domain.Entities
{
    public class PhieuXuatDong
    {
        public long Id { get; set; }

        public long? PhieuXuatId { get; set; }
        public long? VatTuId { get; set; }

        public string? SoLo { get; set; }           // nếu bạn quản lô/batch
        public decimal SoLuong { get; set; }        // 18,3
        public decimal DonGia { get; set; }         // 18,2 (nếu có tính giá)
        public decimal? GiaTri { get; set; }        // 18,2 (SoLuong * DonGia), có thể null nếu chưa tính

        // (tuỳ chọn) Navigation:
        // public virtual PhieuXuat? PhieuXuat { get; set; }
        // public virtual VatTu? VatTu { get; set; }
    }
}
