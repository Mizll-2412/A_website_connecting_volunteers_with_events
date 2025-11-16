using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("TinhNguyenVien_LinhVuc")]
    public class TinhNguyenVien_LinhVuc
    {
        
        [Key]
        [Column("MaTNV")]
        public int MaTNV { get; set; }
        [Key]
        [Column("MaLinhVuc")]
        public int MaLinhVuc { get; set; }

        [ForeignKey("MaTNV")]
        public virtual TinhNguyenVien TinhNguyenVien { get; set; } = null!;

        [ForeignKey("MaLinhVuc")]
        public virtual LinhVuc LinhVuc { get; set; } = null!;
    }
}