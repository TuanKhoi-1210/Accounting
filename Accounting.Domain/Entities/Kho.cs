namespace Accounting.Domain.Entities
{
    public class Kho
    {
        public int Id { get; set; }
        public string Ma { get; set; } = null!;
        public string Ten { get; set; } = null!;

        public DateTime? NgayTao { get; set; }
        public string? NguoiTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public string? NguoiCapNhat { get; set; }
        public bool DaXoa { get; set; }
    }
}
