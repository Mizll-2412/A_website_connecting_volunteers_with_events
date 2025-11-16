using System;
using System.Collections.Generic;

namespace khoaluantotnghiep.DTOs
{
    public class EventHistoryDto
    {
        public int MaSuKien { get; set; }
        public string TenSuKien { get; set; } = string.Empty;
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string DiaChi { get; set; } = string.Empty;
        public string HinhAnh { get; set; } = string.Empty;
        public int? TrangThaiDangKy { get; set; } // 1: Chờ duyệt, 2: Đã duyệt, 3: Từ chối, 4: Đã tham gia
        public string TrangThaiDangKyText { get; set; } = string.Empty;
        public DateTime NgayDangKy { get; set; }
        public bool DaHoanThanh { get; set; }
        public bool DaDanhGia { get; set; }
        public bool CoGiayChungNhan { get; set; }
        public string TenToChuc { get; set; } = string.Empty;
        public int MaToChuc { get; set; }
    }
    
    public class EventHistoryFilterDto
    {
        public int? Nam { get; set; }
        public int? Thang { get; set; }
        public bool? HoanThanh { get; set; }
        public bool? CoGiayChungNhan { get; set; }
    }
    
    public class EventHistoryStatsDto
    {
        public int TongSuKien { get; set; }
        public int SuKienDaHoanThanh { get; set; }
        public int SuKienDangCho { get; set; }
        public int SuKienDaHuy { get; set; }
        public int SoGiayChungNhan { get; set; }
        public Dictionary<int, int> ThongKeSuKienTheoThang { get; set; } = new Dictionary<int, int>();
    }
}
