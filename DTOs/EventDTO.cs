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

        public DateTime? NgayDienRaBatDau { get; set; }

        public DateTime? NgayDienRaKetThuc { get; set; }

        public int? ThoiGianKhoaHuy { get; set; }

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

        public DateTime? NgayDienRaBatDau { get; set; }

        public DateTime? NgayDienRaKetThuc { get; set; }

        public int? ThoiGianKhoaHuy { get; set; }

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
        
        // Thêm các trường formatted và date format pattern
        public string DateFormat { get; set; } = "dd/MM/yyyy HH:mm";
        public string? NgayBatDauFormatted { get; set; }
        public string? NgayKetThucFormatted { get; set; }
        public string? TuyenBatDauFormatted { get; set; }
        public string? TuyenKetThucFormatted { get; set; }
        public string? NgayTaoFormatted { get; set; }
        
        // Trạng thái tuyển dụng (2 trạng thái độc lập)
        public string? TrangThaiTuyen { get; set; }        // "Chưa mở đăng ký", "Đang tuyển", "Đã đủ người", "Hết hạn tuyển", "Đóng"
        public string? TrangThaiTuyenMau { get; set; }     // CSS class: "info", "success", "warning", "danger", "secondary"
        
        // Trạng thái sự kiện
        public string? TrangThaiSuKien { get; set; }       // "Sắp diễn ra", "Đang diễn ra", "Đã kết thúc"
        public string? TrangThaiSuKienMau { get; set; }    // CSS class
        
        // Thông tin chi tiết
        public bool ChoPhepDangKy { get; set; }            // true/false - cho phép đăng ký hay không
        public int SoLuongConLai { get; set; }             // soLuong - soLuongDaDangKy
        
        // Thời gian diễn ra thực tế
        public DateTime? NgayDienRaBatDau { get; set; }
        public DateTime? NgayDienRaKetThuc { get; set; }
        public string? NgayDienRaBatDauFormatted { get; set; }
        public string? NgayDienRaKetThucFormatted { get; set; }
        public int ThoiGianKhoaHuy { get; set; } // Giờ
        
        // Thống kê đăng ký chi tiết (3 loại)
        public int SoLuongDaDuyet { get; set; }      // TrangThai = 1
        public int SoLuongChoDuyet { get; set; }     // TrangThai = 0
        public int TongSoDangKy { get; set; }        // DaDuyet + ChoDuyet
        public int GioiHanDangKy { get; set; }       // SoLuong × 1.5
        
        // Thông tin hủy đăng ký
        public bool CoTheHuyDangKy { get; set; }     // Logic kiểm tra
        public int SoGioConLaiDeHuy { get; set; }    // Cho countdown
    }
}
