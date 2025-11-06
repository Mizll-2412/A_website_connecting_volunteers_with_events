using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace khoaluantotnghiep.DTOs
{
    public class CertificateSampleDto
    {
        public int MaMau { get; set; }
        public int? MaSuKien { get; set; }
        public string? TenSuKien { get; set; }
        public string? TenMau { get; set; }
        public string? MoTa { get; set; }
        public bool IsDefault { get; set; }
        public DateTime? NgayGui { get; set; }
        public string? File { get; set; }
    }

    public class CreateCertificateSampleDto
    {
        public int? MaSuKien { get; set; }

        public string? TenMau { get; set; }

        public string? MoTa { get; set; }

        public bool IsDefault { get; set; }

        [Required(ErrorMessage = "File mẫu giấy chứng nhận là bắt buộc")]
        public IFormFile File { get; set; }
    }

    public class CertificateDto
    {
        public int MaGiayChungNhan { get; set; }
        public int MaMau { get; set; }
        public string? TenMau { get; set; }
        public int MaTNV { get; set; }
        public string? TenTNV { get; set; }
        public int? MaSuKien { get; set; }
        public string? TenSuKien { get; set; }
        public string? TenToChuc { get; set; }
        public DateTime? NgayCap { get; set; }
        public string? FilePath { get; set; }
    }

    public class IssueCertificateDto
    {
        [Required(ErrorMessage = "Mã tình nguyện viên là bắt buộc")]
        public int MaTNV { get; set; }

        [Required(ErrorMessage = "Mã sự kiện là bắt buộc")]
        public int MaSuKien { get; set; }

        [Required(ErrorMessage = "Mã mẫu giấy chứng nhận là bắt buộc")]
        public int MaMau { get; set; }

        public IFormFile? File { get; set; } // Optional, nếu không cung cấp sẽ sử dụng mẫu
    }

    public class CertificateFilterDto
    {
        public int? MaSuKien { get; set; }
        public int? MaTNV { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
