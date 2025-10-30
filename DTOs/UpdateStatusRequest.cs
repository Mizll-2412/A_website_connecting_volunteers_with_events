using System.ComponentModel.DataAnnotations;

namespace khoaluantotnghiep.DTOs
{
    public class UpdateStatusRequest
    {
        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public bool TrangThai { get; set; }
    }
}
