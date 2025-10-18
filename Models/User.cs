using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("TaiKhoan")]
    public class TaiKhoan
    {
        [Key]
        [Column("MaTaiKhoan")]
        public int MaTaiKhoan { get; set; }
        //[Required]
        // [StringLength(100)]
        // [Column("HoTen")]
        // public string HoTen { get; set; }
        [Required]
        [StringLength(100)]
        [Column("Email")]
        public string Email { get; set; }
        [Required]
        [Column("Password")]
        public string Password { get; set; }

        [Required]
        [Column("PasswordSalt")]
        public string PasswordSalt { get; set; }

        [StringLength(50)]
        [Column("VaiTro")]
        public string VaiTro { get; set; }

        [Column("TrangThai")]
        public bool TrangThai { get; set; } = true;

        [Column("NgayTao")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Column("LanDangNhapCuoi")]
        public DateTime? LanDangNhapCuoi { get; set; }
        public virtual TinhNguyenVien? Volunteer { get; set; }
        public virtual ToChuc? Organization { get; set; }

    }
}