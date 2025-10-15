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
        public int MaSuKien { get; set; }
        public DateTime NgayTao { get; set; }
        public string? GhiChu { get; set; }
        public int? TrangThai { get; set; }
        public string? TrangThaiText { get; set; }
        public string? TenTNV { get; set; }
        public string? TenSuKien { get; set; }
    }
}