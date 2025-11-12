using System.ComponentModel.DataAnnotations;

namespace khoaluantotnghiep.DTOs
{
    public class VerifyOrganizationRequest
    {
        [Required(ErrorMessage = "Trạng thái xác minh là bắt buộc")]
        public bool DaXacMinh { get; set; }
        
        public string LyDoTuChoi { get; set; } = "";

        public string? HanhDong { get; set; }
    }
}
