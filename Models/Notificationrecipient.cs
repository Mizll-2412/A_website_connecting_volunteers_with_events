using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("NguoiNhanThongBao")]
    public class NguoiNhanThongBao
    {
        [Key]
        [Column("MaNguoiNhanThongBao")]
        public int MaNguoiNhanThongBao { get; set; }

        [Required]
        [Column("MaThongBao")]
        public int MaThongBao { get; set; }

        [Column("TrangThai")]
        public byte? TrangThai { get; set; }

        [ForeignKey("MaThongBao")]
        public virtual ThongBao ThongBao { get; set; }

        [ForeignKey("MaNguoiNhanThongBao")]
        public virtual TaiKhoan TaiKhoan { get; set; }
    }
}
