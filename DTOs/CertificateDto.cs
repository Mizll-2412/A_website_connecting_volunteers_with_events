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
        public string? FilePath { get; set; }
        public string? TemplateConfig { get; set; }
        public string? BackgroundImage { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class CreateCertificateSampleDto
    {
        public int? MaSuKien { get; set; }

        public string? TenMau { get; set; }

        public string? MoTa { get; set; }

        public bool IsDefault { get; set; }

        public IFormFile? File { get; set; }

        // Tên file ảnh nền (đã upload qua /upload), nếu có
        public string? BackgroundImage { get; set; }
    }

    public class CertificateDto
    {
        public int MaGiayChungNhan { get; set; }
        public int? MaMau { get; set; }
        public string? TenMau { get; set; }
        public int MaTNV { get; set; }
        public string? TenTNV { get; set; }
        public int? MaSuKien { get; set; }
        public string? TenSuKien { get; set; }
        public string? TenToChuc { get; set; }
        public DateTime? NgayCap { get; set; }
        public string? FilePath { get; set; }
        public string? CertificateData { get; set; } // TemplateConfig đã điền data
        public string? BackgroundImage { get; set; } // Ảnh nền
        public int Width { get; set; } = 1200; // Chiều rộng canvas
        public int Height { get; set; } = 800; // Chiều cao canvas
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
