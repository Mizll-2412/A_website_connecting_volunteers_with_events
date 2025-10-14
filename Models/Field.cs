using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("LinhVuc")]
    public class LinhVuc
    {
        [Key]
        [Column("MaLinhVuc")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaLinhVuc { get; set; }

        [Required]
        [StringLength(100)]
        [Column("TenLinhVuc")]
        public string TenLinhVuc { get; set; }

    }
}
