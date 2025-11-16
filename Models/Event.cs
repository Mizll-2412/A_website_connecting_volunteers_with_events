using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("SuKien")]
    public class SuKien
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
        public string TenSuKien { get; set; } = string.Empty;
        [Required]
        [StringLength(500)]
        [Column("NoiDung")]
        public string NoiDung { get; set; } = string.Empty;
        [Column("SoLuong")]
        public int? SoLuong { get; set; }

        [StringLength(255)]
        [Column("DiaChi")]
        public string? DiaChi { get; set; }

        [Column("NgayBatDau")]
        public DateTime? NgayBatDau { get; set; } = DateTime.Now;
        [Column("NgayKetThuc")]
        public DateTime? NgayKetThuc { get; set; }

        [Column("NgayTao")]
        public DateTime? NgayTao { get; set; } = DateTime.Now;

        [Column("NgayTuyen")]
        public DateTime? TuyenBatDau  { get; set; } = DateTime.Now;

        [Column("NgayKetThucTuyen")]
        public DateTime? TuyenKetThuc  { get; set; }

        [StringLength(200)]
        [Column("TrangThai")]
        public string? TrangThai { get; set; }

        [Column("HinhAnh")]
        [StringLength(255)]
        public string? HinhAnh { get; set; }

        [ForeignKey("MaToChuc")]
        public virtual ToChuc Organization { get; set; } = null!;

        public virtual ICollection<SuKien_LinhVuc> SuKien_LinhVucs { get; set; } = new List<SuKien_LinhVuc>();
        public virtual ICollection<SuKien_KyNang> SuKien_KyNangs { get; set; } = new List<SuKien_KyNang>();

    }
}