using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("GiayChungNhan")]
    public class GiayChungNhan
    {
        [Key]
        [Column("MaGiayChungNhan")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaGiayChungNhan { get; set; }

        [Column("MaMau")]
        public int MaMau { get; set; }

        [Column("MaTNV")]
        public int MaTNV { get; set; }

        [Column("MaSuKien")]
        public int MaSuKien { get; set; }

        [Column("NgayCap")]
        public DateTime? NgayCap { get; set; }

        [StringLength(255)]
        [Column("File")]
        public string? File { get; set; }

        [ForeignKey("MaMau")]
        public virtual MauGiayChungNhan MauGiayChungNhan { get; set; }

        [ForeignKey("MaTNV")]
        public virtual TinhNguyenVien TinhNguyenVien { get; set; }

        [ForeignKey("MaSuKien")]
        public virtual SuKien SuKien { get; set; }
    }
}
