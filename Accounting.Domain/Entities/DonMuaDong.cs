namespace Accounting.Domain.Entities
{
    public class DonMuaDong
    {
        public int Id { get; set; }
        public int? DonMuaId { get; set; }
        public int? VatTuId { get; set; }
        public string? KichThuoc { get; set; }
        public string? LoaiGiay { get; set; }
        public string? DinhLuongGsm { get; set; }
        public string? MauIn { get; set; }
        public string? GiaCong { get; set; }

        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }

        public int? ThueSuatId { get; set; }
        public decimal? TienThue { get; set; }
        public decimal? ThanhTien { get; set; }
    }
}
