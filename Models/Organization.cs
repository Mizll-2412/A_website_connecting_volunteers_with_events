using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("Organization")]
    public class Organization
    {
        [Key]
        [Column("MaToChuc")]
        public int MaToChuc { get; set; }

        [Required]
        [Column("MaTaiKhoan")]
        public int MaTaiKhoan { get; set; }

        [Required]
        [StringLength(100)]
        [Column("TenToChuc")]
        public string TenToChuc { get; set; }
        [Required]
        [StringLength(100)]
        [Column("Email")]
        public string Email { get; set; }
        [StringLength(12)]
        [Column("SoDienThoai")]
        public string SoDienThoai { get; set; }

        [StringLength(255)]
        [Column("DiaChi")]
        public string DiaChi { get; set; }

        [Column("NgayTao")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [StringLength(500)]
        [Column("GioiThieu")]
        public string GioiThieu { get; set; }

        [StringLength(500)]
        [Column("AnhDaiDien")]
        public string AnhDaiDien { get; set; }

        [Column("MaGiayTo")]
        public int MaGiayTo { get; set; }

        [ForeignKey("MaTaiKhoan")]
        public virtual User User { get; set; }

        [ForeignKey("MaGiayTo")]
        public virtual LegalDocument LegalDocument { get; set; }
    }
}