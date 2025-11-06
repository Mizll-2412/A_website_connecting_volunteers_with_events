using System.ComponentModel.DataAnnotations;

namespace khoaluantotnghiep.DTOs
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Token là bắt buộc")]
        public string Token { get; set; }
        
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; }
    }
}
