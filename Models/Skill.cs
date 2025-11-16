using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("KyNang")]
    public class KyNang
    {
        [Key]
        [Column("MaKyNang")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaKyNang { get; set; }

        [Required]
        [StringLength(100)]
        [Column("TenKyNang")]
        public string TenKyNang { get; set; } = string.Empty;
    }
}
