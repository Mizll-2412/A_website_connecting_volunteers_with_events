using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.DTOs
{
    public class CreateKyNangRequest
    {
        [Required(ErrorMessage = "Tên kỹ năng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên kỹ năg không được quá 100 ký tự")]
        public string TenKyNang { get; set; } = string.Empty;
    }

    public class UpdateKyNangRequest
    {
        [Required(ErrorMessage = "Tên kỹ năng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên kỹ năng không được quá 100 ký tự")]
        public string TenKyNang { get; set; } = string.Empty;
    }

    public class KyNangResponse
    {
        public int MaKyNang { get; set; }
        public string TenKyNang { get; set; } = string.Empty;
    }
}
