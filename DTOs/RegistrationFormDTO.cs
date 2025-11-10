using System;
using System.ComponentModel.DataAnnotations;

namespace khoaluantotnghiep.DTOs
{
    public class CreateDonDangKyDto
    {
        [Required]
        public int MaTNV { get; set; }

        [Required]
        public int MaSuKien { get; set; }

        [StringLength(255)]
        public string? GhiChu { get; set; }


    }
    public class UpdateDonDangKyDto
    {
        [Required]
        public int TrangThai { get; set; }

        [StringLength(255)]
        public string? GhiChu { get; set; }
    }

    public class DonDangKyResponseDto
    {
        public int MaTNV { get; set; }
        public int MaTaiKhoan { get; set; }
        public int MaSuKien { get; set; }
        public DateTime NgayTao { get; set; }
        public string? GhiChu { get; set; }
        public int? TrangThai { get; set; }
        public string? TrangThaiText { get; set; }
        public string? TenTNV { get; set; }
        public string? TenSuKien { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        
        // Thông tin chi tiết sự kiện
        public EventBasicInfo? Event { get; set; }
    }
    
    public class EventBasicInfo
    {
        public int MaSuKien { get; set; }
        public string? TenSuKien { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string? DiaChi { get; set; }
        public int? MaToChuc { get; set; }
        public int? MaTaiKhoanToChuc { get; set; }
        public string? TrangThai { get; set; }
        public string? TrangThaiHienThi { get; set; }
    }
}