using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("ToChuc")]
    public class ToChuc
    {
        [Key]
        [Column("MaToChuc")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaToChuc { get; set; }

        [Required]
        [Column("MaTaiKhoan")]
        public int MaTaiKhoan { get; set; }

        [StringLength(100)]
        [Column("TenToChuc")]
        public string? TenToChuc { get; set; }
        [Required]
        [StringLength(100)]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;
        [StringLength(12)]
        [Column("SoDienThoai")]
        public string? SoDienThoai { get; set; }

        [StringLength(255)]
        [Column("DiaChi")]
        public string? DiaChi { get; set; }

        [Column("NgayTao")]
        public DateTime? NgayTao { get; set; } = DateTime.Now;

        [StringLength(500)]
        [Column("GioiThieu")]
        public string? GioiThieu { get; set; }

        [StringLength(500)]
        [Column("AnhDaiDien")]
        public string? AnhDaiDien { get; set; }

        [ForeignKey("MaTaiKhoan")]
        public virtual TaiKhoan TaiKhoan { get; set; } = null!;
        public byte? TrangThaiXacMinh { get; set; }
        public string? LyDoTuChoi { get; set; }
        [Column("DiemTrungBinh")]
        public decimal? DiemTrungBinh { get; set; }

        public virtual ICollection<GiayToPhapLy> GiayToPhapLys { get; set; } = new List<GiayToPhapLy>();

    }
}