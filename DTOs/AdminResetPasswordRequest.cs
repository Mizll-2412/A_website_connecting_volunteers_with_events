using System.ComponentModel.DataAnnotations;

namespace khoaluantotnghiep.DTOs
{
    public class AdminResetPasswordRequest
    {
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; } = string.Empty;
    }
}

