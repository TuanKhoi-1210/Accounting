namespace Accounting.Domain.Entities
{
    public class ThueSuat
    {
        public long Id { get; set; }
        public string Ten { get; set; } = null!;
        public decimal TyLe { get; set; }
        public bool DangHoatDong { get; set; }
    }
}
