using System;
using System.ComponentModel.DataAnnotations;

namespace khoaluantotnghiep.DTOs
{
    public class UploadDocument
    {
        [Required]
        public int MaToChuc { get; set; }

        [Required]
        [StringLength(200)]
        public string TenGiayTo { get; set; }

        [Required]
        public IFormFile File { get; set; }
    }
    public class XacMinhToChucDto
    {
        [Required]
        public byte TrangThai { get; set; } // 1: Duyệt, 2: Từ chối

        [StringLength(500)]
        public string? LyDoTuChoi { get; set; }
    }

    public class GiayToPhapLyResponseDto
    {
        public int MaGiayTo { get; set; }
        public int MaToChuc { get; set; }
        public string? TenGiayTo { get; set; }
        public DateTime NgayTao { get; set; }
        public string? File { get; set; }
    }

    public class ToChucXacMinhResponseDto
    {
        public int MaToChuc { get; set; }
        public string? TenToChuc { get; set; }
        public string Email { get; set; }
        public byte? TrangThaiXacMinh { get; set; }
        public string? TrangThaiXacMinhText { get; set; }
        public string? LyDoTuChoi { get; set; }
        public DateTime? NgayTao { get; set; }
        public List<GiayToPhapLyResponseDto>? GiayToPhapLys { get; set; }
    }
}