namespace Accounting.Domain.Entities
{
    public class VatTu
    {
        public int Id { get; set; }
        public string Ma { get; set; } = null!;
        public string Ten { get; set; } = null!;
        public string? KichThuoc { get; set; }
        public string? LoaiGiay { get; set; }
        public string? DinhLuongGsm { get; set; }
        public string? MauIn { get; set; }
        public string? GiaCong { get; set; }

        public int? DonViTinhId { get; set; }

        public DateTime? NgayTao { get; set; }
        public string? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public string? NguoiCapNhat { get; set; }
        public bool DaXoa { get; set; }
    }
}
