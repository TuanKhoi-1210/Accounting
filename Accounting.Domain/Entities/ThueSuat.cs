namespace Accounting.Domain.Entities
{
    public class ThueSuat
    {
        public int Id { get; set; }
        public string Ten { get; set; } = null!;
        public decimal TyLe { get; set; }
        public bool DangHoatDong { get; set; }
    }
}
