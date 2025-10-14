using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("DanhGia")]
    public class DanhGia
    {
        [Key]
        [Column("MaDanhGia")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDanhGia { get; set; }

        [Required]
        [Column("MaNguoiDanhGia")]
        public int MaNguoiDanhGia { get; set; }

        [Required]
        [Column("MaNguoiDuocDanhGia")]
        public int MaNguoiDuocDanhGia { get; set; }

        [Required]
        [Column("MaSuKien")]
        public int MaSuKien { get; set; }

        [Required]
        [Range(1, 5)]
        [Column("DiemSo")]
        public int DiemSo { get; set; }

        [StringLength(500)]
        [Column("NoiDung")]
        public string? NoiDung { get; set; }

        [Column("NgayTao")]
        public DateTime NgayTao { get; set; } = DateTime.Now;
        [ForeignKey("MaNguoiDanhGia")]
        public virtual TaiKhoan NguoiDanhGia { get; set; }

        [ForeignKey("MaNguoiDuocDanhGia")]
        public virtual TaiKhoan NguoiDuocDanhGia { get; set; }

        [ForeignKey("MaSuKien")]
        public virtual SuKien Event { get; set; }
    }
}
