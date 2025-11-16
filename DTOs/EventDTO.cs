using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace khoaluantotnghiep.DTOs
{
    public class CreateSuKienDto
    {
        [Required]
        public int MaToChuc { get; set; }

        [Required]
        [StringLength(100)]
        public string TenSuKien { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string NoiDung { get; set; } = string.Empty;

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
        public string TenSuKien { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string NoiDung { get; set; } = string.Empty;

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
        public string TenSuKien { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty; // Thêm thuộc tính cho SearchService
        public int? SoLuong { get; set; }
        public string? DiaChi { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public DateTime? NgayTao { get; set; }
        public DateTime? TuyenBatDau { get; set; }
        public DateTime? TuyenKetThuc { get; set; }
        public int TrangThai { get; set; } // Chuyển từ string sang int để phù hợp với model
        public string? TrangThaiHienThi { get; set; } // Trạng thái hiển thị tính toán dựa trên ngày và DB
        public string? HinhAnh { get; set; }
        public List<int>? LinhVucIds { get; set; }
        public List<int>? KyNangIds { get; set; }
        // Thêm các thuộc tính cho SearchService
        public string? TenToChuc { get; set; }
        public int? MaTaiKhoanToChuc { get; set; } // MaTaiKhoan của tổ chức để đánh giá
        public bool? TrangThaiXacMinhToChuc { get; set; }
        public List<LinhVucDto>? LinhVucs { get; set; }
        public List<KyNangDto>? KyNangs { get; set; }
        public int? SoLuongDaDangKy { get; set; } // Số lượng đã đăng ký (đã duyệt)
    }
}
