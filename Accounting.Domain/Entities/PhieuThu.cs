using System.Text.Json.Serialization;

namespace Accounting.Domain.Entities
{
    public class PhieuThu
    {
        public long Id { get; set; }
        public string SoCt { get; set; } = default!;
        public DateTime NgayCt { get; set; }
        public string NguoiNop { get; set; } = default!;
        public decimal SoTien { get; set; }
        public string? LyDo { get; set; }

        public DateTime? NgayTao { get; set; }
        public string? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public string? NguoiCapNhat { get; set; }
        public bool DaXoa { get; set; }
        public long? HoaDonBanId { get; set; }

        [JsonIgnore]
        public HoaDonBan? HoaDonBan { get; set; }
    }
}
