using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace khoaluantotnghiep.DTOs
{
    public class CreateDanhGiaDto
    {
        [Required]
        public int MaNguoiDanhGia { get; set; }

        [Required]
        public int MaNguoiDuocDanhGia { get; set; }

        [Required]
        public int MaSuKien { get; set; }

        [Required]
        [Range(1, 5)]
        public int DiemSo { get; set; }

        [StringLength(500)]
        public string? NoiDung { get; set; }
    }

    public class UpdateDanhGiaDto
    {
        [Required]
        [Range(1, 5)]
        public int DiemSo { get; set; }

        [StringLength(500)]
        public string? NoiDung { get; set; }
    }

    public class DanhGiaResponseDto
    {
        public int MaDanhGia { get; set; }
        public int MaNguoiDanhGia { get; set; }
        public string? TenNguoiDanhGia { get; set; }
        public string? VaiTroNguoiDanhGia { get; set; }
        public int MaNguoiDuocDanhGia { get; set; }
        public string? TenNguoiDuocDanhGia { get; set; }
        public string? VaiTroNguoiDuocDanhGia { get; set; }
        public int MaSuKien { get; set; }
        public string? TenSuKien { get; set; }
        public int DiemSo { get; set; }
        public string? NoiDung { get; set; }
        public DateTime NgayTao { get; set; }
    }

    public class ThongKeDanhGiaDto
    {
        public int MaNguoi { get; set; }
        public string? TenNguoi { get; set; }
        public decimal DiemTrungBinh { get; set; }
        public int TongSoDanhGia { get; set; }
        public string? CapBac { get; set; }
        public int TongSuKienThamGia { get; set; }
        public List<DanhGiaResponseDto>? DanhSachs { get; set; }
    }
    
    public class CapBacDto
    {
        public string Ten { get; set; } = string.Empty;
        public decimal DiemTuongUng { get; set; }
        public string? MoTa { get; set; }
    }

    public class BulkCreateDanhGiaDto
    {
        [Required]
        public int MaSuKien { get; set; }

        [Required]
        [Range(1, 5)]
        public int DiemSo { get; set; }

        [StringLength(500)]
        public string? NoiDung { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Phải có ít nhất một tình nguyện viên")]
        public List<int> MaTNVs { get; set; } = new List<int>();
    }
}