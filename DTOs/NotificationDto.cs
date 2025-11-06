using System;
using System.ComponentModel.DataAnnotations;

namespace khoaluantotnghiep.DTOs
{
    public class CreateNotificationDto
    {
        [Required]
        public int MaNguoiTao { get; set; }
        
        [Required]
        public int PhanLoai { get; set; } // 1: Thông báo hệ thống, 2: Thông báo sự kiện, 3: Thông báo đánh giá, 4: Thông báo chứng nhận
        
        [Required]
        [StringLength(1000)]
        public string NoiDung { get; set; }
        
        [Required]
        public List<int> MaNguoiNhans { get; set; }
    }
    
    public class NotificationResponseDto
    {
        public int MaThongBao { get; set; }
        public int MaNguoiTao { get; set; }
        public string TenNguoiTao { get; set; }
        public string AnhDaiDienNguoiTao { get; set; }
        public int PhanLoai { get; set; }
        public string PhanLoaiText { get; set; }
        public string NoiDung { get; set; }
        public DateTime NgayGui { get; set; }
        public byte? TrangThai { get; set; } // 0: Chưa đọc, 1: Đã đọc
        public string TrangThaiText { get; set; }
        public int MaNguoiNhanThongBao { get; set; }
    }
    
    public class NotificationCountDto
    {
        public int TongSo { get; set; }
        public int ChuaDoc { get; set; }
        public int DaDoc { get; set; }
    }
    
    public class UpdateNotificationStatusDto
    {
        [Required]
        public int MaThongBao { get; set; }
        
        [Required]
        public byte TrangThai { get; set; }
    }

    public class InviteEventDto
    {
        [Required]
        public int MaNguoiNhan { get; set; }
        
        [Required]
        public int MaSuKien { get; set; }
    }
}
