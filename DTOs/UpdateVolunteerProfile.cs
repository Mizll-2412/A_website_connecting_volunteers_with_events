using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace khoaluantotnghiep.DTOs
{
    public class UpdateTNVDto
    {
        [StringLength(100)]
        public string? HoTen { get; set; }

        [EmailAddress]
        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(12)]
        public string? CCCD { get; set; }

        [StringLength(12)]
        public string? SoDienThoai { get; set; }

        public DateTime? NgaySinh { get; set; }

        [StringLength(10)]
        public string? GioiTinh { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }

        [StringLength(500)]
        public string? GioiThieu { get; set; }

        [StringLength(500)]
        public string? AnhDaiDien { get; set; }

        public List<int>? LinhVucIds { get; set; }
        public List<int>? KyNangIds { get; set; }
    }
    
    public class CreateTNVDto
    {
        [Required]
        public int MaTaiKhoan { get; set; }

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; }

        [EmailAddress]
        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(12)]
        public string? CCCD { get; set; }

        [StringLength(12)]
        public string? SoDienThoai { get; set; }

        public DateTime? NgaySinh { get; set; }

        [StringLength(10)]
        public string? GioiTinh { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }

        [StringLength(500)]
        public string? GioiThieu { get; set; }

        [StringLength(500)]
        public string? AnhDaiDien { get; set; }

        public List<int>? LinhVucIds { get; set; }
        public List<int>? KyNangIds { get; set; }
    }
    
    public class TinhNguyenVienResponseDto
    {
        public int MaTNV { get; set; }
        public int MaTaiKhoan { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string? CCCD { get; set; }
        public string? SoDienThoai { get; set; }
        public string? NgaySinh { get; set; } // Đổi thành string để format yyyy-MM-dd
        public string? GioiTinh { get; set; }
        public string? DiaChi { get; set; }
        public string? GioiThieu { get; set; }
        public string? AnhDaiDien { get; set; }
        public decimal? DiemTrungBinh { get; set; }
        public string? CapBac { get; set; }
        public int? TongSuKienThamGia { get; set; }
        public List<int>? LinhVucIds { get; set; }
        public List<int>? KyNangIds { get; set; }
        public List<KyNangDto>? KyNangs { get; set; }
        public List<LinhVucDto>? LinhVucs { get; set; }
    }

    public class KyNangDto
    {
        public int MaKyNang { get; set; }
        public string? TenKyNang { get; set; }
    }

    public class LinhVucDto
    {
        public int MaLinhVuc { get; set; }
        public string? TenLinhVuc { get; set; }
    }

    public class UpdateAnhDaiDienDto
    {
        [Required]
        public IFormFile AnhFile { get; set; }
    }
}