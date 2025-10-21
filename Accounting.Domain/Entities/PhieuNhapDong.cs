namespace Accounting.Domain.Entities
{
    public class PhieuNhapDong
    {
        public int Id { get; set; }
        public int? PhieuNhapId { get; set; }
        public int? VatTuId { get; set; }

        public string? SoLo { get; set; }
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal? GiaTri { get; set; }
    }
}
