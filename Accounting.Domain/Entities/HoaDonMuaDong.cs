namespace Accounting.Domain.Entities
{
    public class HoaDonMuaDong
    {
        public int Id { get; set; }
        public int? HoaDonMuaId { get; set; }
        public int? VatTuId { get; set; }

        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }

        public int? ThueSuatId { get; set; }
        public decimal? TienThue { get; set; }
        public decimal? ThanhTien { get; set; }
    }
}
