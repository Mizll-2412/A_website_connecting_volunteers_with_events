using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.DTOs
{
    public class CreateLinhVucRequest
    {
        [Required(ErrorMessage = "Tên lĩnh vực là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên lĩnh vực không được quá 100 ký tự")]
        public string TenLinhVuc { get; set; }
    }

    public class UpdateLinhVucRequest
    {
        [Required(ErrorMessage = "Tên lĩnh vực là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên lĩnh vực không được quá 100 ký tự")]
        public string TenLinhVuc { get; set; }
    }

    public class LinhVucResponse
    {
        public int MaLinhVuc { get; set; }
        public string TenLinhVuc { get; set; }
    }
}
