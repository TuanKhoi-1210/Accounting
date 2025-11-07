using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounting.Domain.Entities
{
    public class PhieuXuat
    {
        public long Id { get; set; }

        public string SoCt { get; set; } = null!;
        // Số chứng từ xuất (ví dụ PX-20251107-001)
        public long? KhoId { get; set; }
        // (tuỳ mô hình kho)
        public long? LenhSanXuatId { get; set; }    // nếu xuất theo Lệnh SX (tuỳ bạn dùng)

        public DateTime NgayXuat { get; set; }      // Ngày chứng từ / ngày xuất
        public string? GhiChu { get; set; }

        public long? DonBanId { get; set; }

        public DateTime? NgayTao { get; set; }
        public string? NguoiTao { get; set; }

        public virtual ICollection<PhieuXuatDong> Dong { get; set; } = new List<PhieuXuatDong>();
    }
}
