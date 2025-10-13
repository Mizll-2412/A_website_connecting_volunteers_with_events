using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace khoaluantotnghiep.Models
{
    [Table("Volunteer")]
    public class Volunteer
    {
        [Key]
        [Column("MaTNV")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaTNV { get; set; }

        [Required]
        [Column("MaTaiKhoan")]
        public int MaTaiKhoan { get; set; }

        [Required]
        [StringLength(100)]
        [Column("HoTen")]
        public string HoTen { get; set; }

        
        [Column("NgaySinh")]
        public DateOnly NgaySinh { get; set; }

        [Column("GioiTinh")]
        public string GioiTinh { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Email")]
        public string Email { get; set; }
         [StringLength(12)]
        [Column("CCCD")]
        public string CCCD { get; set; }

        [StringLength(255)]
        [Column("DiaChi")]
        public string DiaChi { get; set; }

        [StringLength(500)]
        [Column("GioiThieu")]
        public string GioiThieu { get; set; }
        [StringLength(500)]
        [Column("AnhDaiDien")]
        public string AnhDaiDien { get; set; }

        [ForeignKey("MaTaiKhoan")]
        public virtual User User { get; set; }
    }
}