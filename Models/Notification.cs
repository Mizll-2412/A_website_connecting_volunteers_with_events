using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("ThongBao")]
    public class ThongBao
    {
        [Key]
        [Column("MaThongBao")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaThongBao { get; set; }
        [Required]
        [Column("MaTaiKhoan")]
        public int MaNguoiTao { get; set; }
        [Required]
        [Column("PhanLoai")]
        public int PhanLoai { get; set; }

        [Required]
        [StringLength(1000)]
        [Column("NoiDung")]
        public string NoiDung { get; set; } = string.Empty;

        [Column("NgayGui")]
        public DateTime NgayGui { get; set; } = DateTime.Now;

        [ForeignKey("MaNguoiTao")]
        public virtual TaiKhoan TaiKhoan { get; set; } = null!;
    }
}
