using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("Event")]
    public class Event
    {
        [Key]
        [Column("MaSuKien")]
        public int MaSuKien { get; set; }

        [Required]
        [Column("MaToChuc")]
        public int MaToChuc { get; set; }

        [Required]
        [StringLength(100)]
        [Column("TenSuKien")]
        public string TenSuKien { get; set; }
        [Required]
        [StringLength(500)]
        [Column("NoiDung")]
        public string NoiDung { get; set; }
        [Column("SoLuong")]
        public int SoLuong { get; set; }

        [StringLength(255)]
        [Column("DiaChi")]
        public string DiaChi { get; set; }

        [Column("NgayBatDau")]
        public DateTime NgayBatDau { get; set; } = DateTime.Now;
        [Column("NgayKetThuc")]
        public DateTime NgayKetThuc { get; set; }

        [Column("NgayTao")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Column("NgayTuyen")]
        public DateTime NgayTuyen { get; set; } = DateTime.Now;

        [Column("NgayKetThucTuyen")]
        public DateTime NgayKetThucTuyen { get; set; }

        [StringLength(500)]
        [Column("TrangThai")]
        public string TrangThai { get; set; }

        [ForeignKey("MaToChuc")]
        public virtual Organization Organization { get; set; }

    }
}