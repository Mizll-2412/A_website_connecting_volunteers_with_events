using System;
using System.Collections.Generic;

namespace khoaluantotnghiep.DTOs
{
    public class EventRecommendationDto
    {
        public int MaSuKien { get; set; }
        public string TenSuKien { get; set; }
        public string MoTa { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string DiaChi { get; set; }
        public string HinhAnh { get; set; }
        public int MaToChuc { get; set; }
        public string TenToChuc { get; set; }
        public decimal? DiemTrungBinhToChuc { get; set; }
        public List<string> KyNangs { get; set; }
        public List<string> LinhVucs { get; set; }
        public decimal MatchScore { get; set; } // Điểm phù hợp
    }
    
    public class RecommendationRequestDto
    {
        public int MaTNV { get; set; }
        public int? MaxResults { get; set; } = 10;
        public List<int>? LinhVucPreferences { get; set; } // Ưu tiên lĩnh vực này
        public double? LocationWeight { get; set; } = 0.3; // Trọng số cho vị trí địa lý
        public double? SkillWeight { get; set; } = 0.4; // Trọng số cho kỹ năng phù hợp
        public double? InterestWeight { get; set; } = 0.3; // Trọng số cho lĩnh vực quan tâm
    }
}
