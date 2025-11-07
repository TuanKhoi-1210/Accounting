namespace Accounting.Domain.Entities
{
    public class PhieuNhapDong
    {
        public long Id { get; set; }
        public long? PhieuNhapId { get; set; }
        public long? VatTuId { get; set; }

        public string? SoLo { get; set; }
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal? GiaTri { get; set; }
    }
}
