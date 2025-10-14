using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("SuKien_LinhVuc")]
    public class SuKien_LinhVuc
    {
        
        [Key]
        [Column("MaSuKien")]
        public int MaSuKien { get; set; }
        [Key]
        [Column("MaLinhVuc")]
        public int MaLinhVuc { get; set; }

        [ForeignKey("MaSuKien")]
        public virtual SuKien SuKien { get; set; }

        [ForeignKey("MaLinhVuc")]
        public virtual LinhVuc LinhVuc { get; set; }
    }
}