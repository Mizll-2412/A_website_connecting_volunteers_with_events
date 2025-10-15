using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("DonDangKy")]
    public class DonDangKy
    {
        [Key, Column("MaTNV")]
        public int MaTNV { get; set; }

        [Required]
        [Column("MaSuKien")]
        public int MaSuKien { get; set; }

        [Column("NgayTao")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [StringLength(255)]
        [Column("GhiChu")]
        public string? GhiChu { get; set; }

        [Column("TrangThai")]
        public int? TrangThai { get; set; }

        [ForeignKey("MaTNV")]
        public virtual TinhNguyenVien TinhNguyenVien { get; set; }

        [ForeignKey("MaSuKien")]
        public virtual SuKien SuKien { get; set; }

    }
}
