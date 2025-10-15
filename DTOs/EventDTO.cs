using System;
using System.ComponentModel.DataAnnotations;

namespace khoaluantotnghiep.DTOs
{
    public class CreateSuKienDto
    {
        [Required]
        public int MaToChuc { get; set; }

        [Required]
        [StringLength(100)]
        public string TenSuKien { get; set; }

        [Required]
        [StringLength(500)]
        public string NoiDung { get; set; }

        public int? SoLuong { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }

        public DateTime? NgayBatDau { get; set; }

        public DateTime? NgayKetThuc { get; set; }

        public DateTime? TuyenBatDau { get; set; }

        public DateTime? TuyenKetThuc { get; set; }


        [StringLength(200)]
        public string? TrangThai { get; set; }
        [StringLength(500)]
        public string? HinhAnh { get; set; }
        public List<int>? LinhVucIds { get; set; }

        public List<int>? KyNangIds { get; set; }
    }

    public class UpdateSuKienDto
    {
        [Required]
        [StringLength(100)]
        public string TenSuKien { get; set; }

        [Required]
        [StringLength(500)]
        public string NoiDung { get; set; }

        public int? SoLuong { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }

        public DateTime? NgayBatDau { get; set; }

        public DateTime? NgayKetThuc { get; set; }

        public DateTime? TuyenBatDau { get; set; }

        public DateTime? TuyenKetThuc { get; set; }

        [StringLength(200)]
        public string? TrangThai { get; set; }

        [StringLength(500)]
        public string? HinhAnh { get; set; }

        public List<int>? LinhVucIds { get; set; }

        public List<int>? KyNangIds { get; set; }
    }

    public class SuKienResponseDto
    {
        public int MaSuKien { get; set; }
        public int MaToChuc { get; set; }
        public string TenSuKien { get; set; }
        public string NoiDung { get; set; }
        public int? SoLuong { get; set; }
        public string? DiaChi { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public DateTime? NgayTao { get; set; }
        public DateTime? TuyenBatDau { get; set; }
        public DateTime? TuyenKetThuc { get; set; }
        public string? TrangThai { get; set; }
        public string? HinhAnh { get; set; }
        public List<int>? LinhVucIds { get; set; }
        public List<int>? KyNangIds { get; set; }
    }
}
