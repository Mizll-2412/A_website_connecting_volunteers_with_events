using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("TokenResetMatKhau")]
    public class TokenResetMatKhau
    {
        [Key]
        [Column("MaToken")]
        public int MaToken { get; set; }

        [Required]
        [Column("MaTaiKhoan")]
        public int MaTaiKhoan { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Token")]
        public string Token { get; set; } = string.Empty;

        [Required]
        [Column("NgayHetHan")]
        public DateTime NgayHetHan { get; set; }

        [Required]
        [Column("DaSuDung")]
        public bool DaSuDung { get; set; }

        [ForeignKey("MaTaiKhoan")]
        public virtual TaiKhoan TaiKhoan { get; set; } = null!;
    }
}
