using System.ComponentModel.DataAnnotations;

namespace khoaluantotnghiep.DTOs
{
    public class UpdateRoleRequest
    {
        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public string VaiTro { get; set; } = string.Empty;
    }
}
