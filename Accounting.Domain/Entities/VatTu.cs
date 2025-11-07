using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounting.Domain.Entities
{
    [Table("vat_tu", Schema = "acc")]
    public class VatTu
    {
        [Key]
        public long Id { get; set; }                // DB hiện dùng long IDENTITY

        [Required, MaxLength(50)]
        public string Ma { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Ten { get; set; } = null!;

        public long? DonViTinhId { get; set; }

        public decimal? NguongTon { get; set; }

        public bool DaXoa { get; set; }

        [Column("is_thanh_pham")]
        public bool IsThanhPham { get; set; } = false;
    }
}