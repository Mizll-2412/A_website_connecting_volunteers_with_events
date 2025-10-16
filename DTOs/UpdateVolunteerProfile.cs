using System;
using System.ComponentModel.DataAnnotations;

namespace khoaluantotnghiep.DTOs
{
    public class UpdateTinhNguyenVienDto
    {
        [Required]
        [StringLength(100)]
        public string HoTen { get; set; }

        public DateOnly? NgaySinh { get; set; }

        [StringLength(20)]
        public string? GioiTinh { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(12)]
        public string? CCCD { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }

        [StringLength(500)]
        public string? GioiThieu { get; set; }

        [StringLength(500)]
        public string? AnhDaiDien { get; set; }

        public List<int>? LinhVucIds { get; set; }

        public List<int>? KyNangIds { get; set; }
    }
     public class CreateTinhNguyenVienDto
    {
        [Required]
        public int MaTaiKhoan { get; set; }

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; }

        public DateOnly? NgaySinh { get; set; }

        [StringLength(20)]
        public string? GioiTinh { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(12)]
        public string? CCCD { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }

        [StringLength(500)]
        public string? GioiThieu { get; set; }

        [StringLength(500)]
        public string? AnhDaiDien { get; set; }
        public decimal? DiemTrungBinh { get; set; }


        public List<int>? LinhVucIds { get; set; }

        public List<int>? KyNangIds { get; set; }
    }
    public class TinhNguyenVienResponseDto
    {
        public int MaTaiKhoan { get; set; }
        public int MaTNV { get; set; }
        public string HoTen { get; set; }
        public DateOnly? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string Email { get; set; }
        public string? CCCD { get; set; }
        public string? DiaChi { get; set; }
        public string? GioiThieu { get; set; }
        public string? AnhDaiDien { get; set; }
        public decimal? DiemTrungBinh { get; set; }

        public List<int>? LinhVucIds { get; set; }
        public List<int>? KyNangIds { get; set; }
    }

    public class UpdateAnhDaiDienDto
    {
        [Required]
        public IFormFile AnhFile { get; set; }
    }
}