using System.ComponentModel.DataAnnotations;

namespace khoaluantotnghiep.DTOs
{
    public class RequestVerificationDto
    {
        [Required]
        public int MaToChuc { get; set; }
        
        [StringLength(500)]
        public string? MoTa { get; set; }
    }
    
    public class VerificationStatusResponseDto
    {
        public int MaToChuc { get; set; }
        public string? TenToChuc { get; set; }
        public byte? TrangThaiXacMinh { get; set; } // 0: Chưa yêu cầu, 1: Chờ xác minh, 2: Đã xác minh, 3: Từ chối
        public string? TrangThaiXacMinhText { get; set; }
        public string? LyDoTuChoi { get; set; }
        public DateTime? NgayYeuCau { get; set; }
        public bool DaGuiYeuCauXacMinh { get; set; }
    }
}
