using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("TinhNguyenVien_KyNang")]
    public class TinhNguyenVien_KyNang
    {
        
        [Key]
        [Column("MaTNV")]
        public int MaTNV { get; set; }
        [Key]
        [Column("MaKyNang")]
        public int MaKyNang { get; set; }

        [ForeignKey("MaTNV")]
        public virtual TinhNguyenVien TinhNguyenVien { get; set; } = null!;

        [ForeignKey("MaKyNang")]
        public virtual KyNang KyNang { get; set; } = null!;
    }
}