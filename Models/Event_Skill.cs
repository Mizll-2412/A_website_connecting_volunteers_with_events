using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("SuKien_KyNang")]
    public class SuKien_KyNang
    {
        
        [Key]
        [Column("MaSuKien")]
        public int MaSuKien { get; set; }
        [Key]
        [Column("MaKyNang")]
        public int MaKyNang { get; set; }

        [ForeignKey("MaSuKien")]
        public virtual SuKien SuKien { get; set; } = null!;

        [ForeignKey("MaKyNang")]
        public virtual KyNang KyNang { get; set; } = null!;
    }
}