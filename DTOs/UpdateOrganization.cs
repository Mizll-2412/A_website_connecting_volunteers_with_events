using System;
using System.ComponentModel.DataAnnotations;

namespace khoaluantotnghiep.DTOs
{
    public class UpdateToChucDto
    {
        [StringLength(100)]
        public string? TenToChuc { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(12)]
        public string? SoDienThoai { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }

        [StringLength(500)]
        public string? GioiThieu { get; set; }

        [StringLength(500)]
        public string? AnhDaiDien { get; set; }
    }

    public class ToChucResponseDto
    {
        public int MaToChuc { get; set; }
        public string? TenToChuc { get; set; }
        public string Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public DateTime? NgayTao { get; set; }
        public string? GioiThieu { get; set; }
        public string? AnhDaiDien { get; set; }
    }
}
